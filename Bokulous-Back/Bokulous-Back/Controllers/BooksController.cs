using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBokulousDbService _bokulousDbService;
    private readonly IBokulousMailService _bokulousMailService;
    private UserHelpers UserHelpers;
    private BookHelpers BookHelper;

    public BooksController(IBokulousDbService bokulousDbService, IBokulousMailService bokulousMailService)
    {
        _bokulousDbService = bokulousDbService;
        _bokulousMailService = bokulousMailService;
        UserHelpers = new(_bokulousDbService);
        BookHelper = new(_bokulousDbService);
    }

    [HttpGet("GetBooks")]
    public async Task<ActionResult<List<Book>>> GetBooks()
    {
        var allBooks = await _bokulousDbService.GetBookAsync();
        if (allBooks is null)
            return NotFound();

        var books = allBooks.Where(x => x.InStorage != 0).ToList();

        return Ok(books);
    }

    [HttpGet("GetBook/{id:length(24)}")]
    public async Task<ActionResult<Book>> GetBook(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest();

        var book = await _bokulousDbService.GetBookAsync(id);

        if (book is null)
            return NotFound();

        return Ok(book);
    }

    [HttpPost("AddBook")]
    public async Task<IActionResult> AddBook(Book newBook)
    {
        if (newBook is null)
            return BadRequest();

        await _bokulousDbService.CreateBookAsync(newBook);
        return CreatedAtAction(nameof(AddBook), new { id = newBook.Id }, newBook);
            
    }

    [HttpPut("UpdateBook/{id:length(24)}")]
    public async Task<IActionResult> UpdateBook(string id, Book updatedBook)
    {
        if(updatedBook is null || string.IsNullOrEmpty(id))
            return BadRequest();

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
        if (string.IsNullOrEmpty(id))
            return BadRequest();

        var book = await _bokulousDbService.GetBookAsync(id);

        if (book is null)
            return NotFound();

        if (book.InStorage > 0)
            book.InStorage--;

        await _bokulousDbService.UpdateBookAsync(id, book);

        return Ok(book);
    }

    [HttpPut("AddBookToCategory")]
    public async Task<ActionResult> AddBookToCategory(string bookId, Category category)
    {
        if (string.IsNullOrEmpty(bookId))
            return BadRequest();

        if (category is null)
            return BadRequest();

        var books = await _bokulousDbService.GetBookAsync(bookId);

        if (books is null)
            return BadRequest();

        for (int i = 0; i < books.Categories.Length; i++)
        {
            if (books.Categories[i] == category.Name)
                return BadRequest();
        }

        books.Categories = new List<string>(books.Categories) { category.Name }.ToArray();
        await _bokulousDbService.UpdateBookAsync(books.Id, books);
        return Ok(books);
    }

    [HttpPost("AddCategory/{category}")]
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

    [HttpPut("UpdateCategory/{newName}")]
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
        if (category is null)
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

        var response = BookHelper.RemoveCategoryFromBooks(booksInCategory, category).Result as StatusCodeResult;

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

    [HttpPut("BuyBook")]
    public async Task<ActionResult> BuyBook(string bookId, string buyerId, string password)
    {
        var book = await _bokulousDbService.GetBookAsync(bookId);
        var buyer = await _bokulousDbService.GetUserAsync(buyerId);

        if (buyer is null)
            return NotFound("Buyer not found");

        if (book is null)
            return NotFound("Book not found");

        if (buyer.Password != password)
            return new StatusCodeResult(StatusCodes.Status403Forbidden);

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

    [HttpGet("GetBooksByAuthor")]
    public async Task<ActionResult<List<Book>>> GetBooksByAuthor(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            return NotFound("Missing a keyword");

        var books = await _bokulousDbService.GetBookAsync();
        if (books.Count == 0 || books is null)
            return NotFound("No books found by this author");

        var book = books.Where(x => x.Authors.Any(y => y.Contains(keyword))).ToList();

        if (book.Count == 0 || book is null)
            return NotFound("No books mathing the author found");

        return Ok(book);
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
    [HttpGet("GetBooksFiltered")]
    public async Task<ActionResult<List<Book>>> GetBookByAdvancedFilter(string? title = null, string? author = null, string? category = null, string? language = null, int? priceMin = null, int? priceMax = null, int? yearMin = null, int? yearMax = null)
    {
        var result = await _bokulousDbService.GetBookAsync();


        if (language is not null)
        {
            result = result.Where(x => x.Language.Contains(language)).ToList();
        }

        if (category is not null)
        {
            result = result.Where(x => x.Categories.Contains(category)).ToList();
        }

        if (title is not null)
        {
            result = result.Where(x => x.Title.Contains(title)).ToList();
        }

        if (author is not null)
        {
            result = result.Where(x => x.Authors.Contains(author)).ToList();
        }

        if (priceMin is not null)
        {
            result = result.Where(x => x.Price >= priceMin).ToList();
        }

        if (priceMax is not null)
        {
            result = result.Where(x => x.Price <= priceMax).ToList();
        }

        if (yearMin is not null)
        {
            result = result.Where(x => x.Published >= yearMin).ToList();
        }

        if (yearMax is not null)
        {
            result = result.Where(x => x.Published <= yearMax).ToList();
        }

        if (result is null)
            return NotFound("No books found");

        return Ok(result);
    }
}