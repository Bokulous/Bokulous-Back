using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Bokulous_Back.Helpers;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BokulousDbService _bokulousDbService;
    private BookHelpers bookHelper;

    public BooksController(BokulousDbService bokulousDbService)
    {
        _bokulousDbService = bokulousDbService;
        bookHelper = new (_bokulousDbService);
    }

[HttpGet("GetBooks")]
    public async Task<List<Book>> GetBooks() =>
        await _bokulousDbService.GetBookAsync();

    [HttpGet("GetBook/{id:length(24)}")]
    public async Task<ActionResult<Book>> GetBook(string id)
    {
        var book = await _bokulousDbService.GetBookAsync(id);

        if (book is null)
            return NotFound();

        return Ok(book);
    }

    [HttpPost("AddBook")]
    public async Task<IActionResult> AddBook(Book newBook)
    {
        var books = await _bokulousDbService.GetBookAsync();
        var book = books.FirstOrDefault(x => x.ISBN == newBook.ISBN && x.Seller == newBook.Seller);
        if (book is null)
        {
            await _bokulousDbService.CreateBookAsync(newBook);
        }
        else
        {
            book.InStorage++;
            await _bokulousDbService.UpdateBookAsync(book.Id, book);
        }

        return CreatedAtAction(nameof(AddBook), new { id = newBook.Id }, newBook);
    }

    [HttpPut("UpdateBook/{id:length(24)}")]
    public async Task<IActionResult> UpdateBook(string id, Book updatedBook)
    {
        var book = await _bokulousDbService.GetBookAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedBook.Id = book.Id;

        await _bokulousDbService.UpdateBookAsync(id, updatedBook);

        return Ok();
    }

    [HttpPut("DeleteBook/{id:length(24)}")]
    public async Task<IActionResult> DeleteBook(string id)
    {
        var book = await _bokulousDbService.GetBookAsync(id);

        if (book is null)
            return NotFound();

        if (book.InStorage > 0)
            book.InStorage--;

        await _bokulousDbService.UpdateBookAsync(id, book);

        return Ok();
    }

    [HttpPost("BuyBook/{bookId:length(24)}")]
    public async Task<IActionResult> BuyBook(string bookId, string buyerId, string password)
    {
        var book = await _bokulousDbService.GetBookAsync(bookId);
        var buyer = await _bokulousDbService.GetUserAsync(buyerId);

        if (buyer is null)
            return NotFound("Buyer not found");

        if (book is null)
            return NotFound("Book not found");

        if (buyer.Password != password)
            return Forbid("Big nono");

        if (book.Seller.Id == buyerId)
            return BadRequest("Seller = Buyer");

        if (book.InStorage < 1)
            return BadRequest("0 books in storage");

        book.InStorage--;
        await _bokulousDbService.UpdateBookAsync(book.Id, book);
        // TODO: add method for sending mock-emails :)
        // maila orderbekräftelse till köpare
        // ev. maila till säljare(admin eller säljare av begagnad bok)

        return Ok();
    }

    [HttpPost("GetBooksByAuthor")] // har skrivit metoden i service för att inte behöva dependency injecta bookcollection
    public async Task<ActionResult> GetBooksByAuthor(string keyword)
    {
        var books = await _bokulousDbService.GetBooksAsyncByAuthor(keyword);

        return Ok(books);
    }

    [HttpGet("GetCategories")]
    public async Task<ActionResult<List<Category>>> GetCategories()
    {
        var categories = await _bokulousDbService.GetCategoryAsync();
        if (categories.Count == 0 || categories is null)
            return NotFound("No categories found");

        return Ok(categories);
    }

    [HttpGet("GetCategoriesByKeyword")]
    public async Task<ActionResult<List<Category>>> GetCategoriesByKeyword(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            return NotFound("Missing a keyword");

        var categories = await _bokulousDbService.GetCategoryAsync();
        if (categories.Count == 0 || categories is null)
            return NotFound("No categories found");

        var cat = categories.Where(x => x.Name.Contains(keyword)).ToList();

        if (cat.Count == 0 || cat is null)
            return NotFound("No categories mathing the name found");

        return Ok(cat);
    }

    [HttpGet("GetBooksByCategory")]
    public async Task<ActionResult<List<Category>>> GetBooksByCategory(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            return NotFound("Missing a keyword");

        var allBooks = await _bokulousDbService.GetBookAsync();
        if (allBooks.Count == 0 || allBooks is null)
            return NotFound("No books found");

        var books = allBooks.Where(x => x.Categories.Contains(keyword)).ToList();

        if (books.Count == 0 || books is null)
            return NotFound("No books found in that category");

        return Ok(books);
    }

    //behöver den verkligen ta emot ett Category-objekt? Räcker det inte med en string på namnet på kategorin man vill skapa?
    [HttpGet("AddCategory")]
    public async Task<ActionResult> AddCategory(string category)
    {
        if (string.IsNullOrEmpty(category))
            return BadRequest();

        var allCategories = await _bokulousDbService.GetCategoryAsync();
        var categories = allCategories.FirstOrDefault(x => x.Name == category);

        if (categories is not null)
            return BadRequest("Category already exists");

        Category newCategory = new()
        {
            Name = category
        };

        await _bokulousDbService.CreateCategoryAsync(newCategory);

        return Ok();
    }

    //Category object eller bara id? 
    [HttpPut("UpdateCategory")]
    public async Task<IActionResult> UpdateCategory(Category category, string newName)
    {
        if (category is null || string.IsNullOrEmpty(newName))
            return BadRequest();

        var cat = await _bokulousDbService.GetCategoryAsync(category.Id);

        if (cat is null)
            return NotFound("The category does not exist");

        cat.Name = newName;

        await _bokulousDbService.UpdateCategoryAsync(cat.Id, cat);

        return Ok();
    }

    [HttpDelete("DeleteCategory")]
    public async Task<ActionResult> DeleteCategory(Category category)
    {
        if(category is null)
            return BadRequest();

        var cat = await _bokulousDbService.GetCategoryAsync(category.Id);
        if (cat is null)
            return NotFound("Category dont exist");

        var books = await _bokulousDbService.GetBookAsync();
        var booksInCategory = books.Where(x => x.Categories.Contains(category.Name)).ToList();

        if (booksInCategory.Count == 0 || booksInCategory is null)
        {
            await _bokulousDbService.RemoveCategoryAsync(category.Id);
            return Ok();
        }
        
        var response = bookHelper.RemoveCategoryFromBooks(booksInCategory, category).Result as StatusCodeResult;

        if (response.StatusCode == 200)
        {
            await _bokulousDbService.RemoveCategoryAsync(category.Id);
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpPut("UploadImage")]
    public async Task<IActionResult> UploadImage(string id, string imagePath)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }
        var book = await _bokulousDbService.GetBookAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(imagePath))
        {
            book.BookCover = System.IO.File.ReadAllBytes(imagePath);
            await _bokulousDbService.UpdateBookAsync(id, book);
        }

        return Ok();
    }

    //public async Task<ActionResult<Image>> LoadImage(string id)
    //{
    //    var book = await _bokulousDbService.GetBookAsync(id);

    //    if (book is null)
    //    {
    //        return NotFound();
    //    }
    //    if (book.BookCover == null || book.BookCover.Length == 0)
    //    {
    //        return NotFound("Bild saknas");
    //    }

    //    Image img;
    //    using (MemoryStream ms = new MemoryStream(book.BookCover))
    //    {
    //        img = Image.FromStream(ms);
    //        //return img;
    //    }

    //    return img;     
    //}
}