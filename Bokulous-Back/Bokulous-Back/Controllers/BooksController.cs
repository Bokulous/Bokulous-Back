using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BokulousDbService _bokulousDbService;

    public BooksController(BokulousDbService bokulousDbService) =>
        _bokulousDbService = bokulousDbService;

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

    [HttpPost("GetBooksByAuthor")] // har skrivit metoden i service för att inte behöva dependency injecta bookcollection
    public async Task<ActionResult> GetBooksByAuthor(string keyword)
    {
        var books = await _bokulousDbService.GetBooksAsyncByAuthor(keyword);

        return Ok(books);
    }
    //onödig?
    //[HttpGet("GetBooksFiltered")]
    //public async Task<ActionResult<List<Book>>> GetBookByKeyword(string keyword)
    //{
    //    if (string.IsNullOrEmpty(keyword))
    //        return NotFound("Missing a keyword");

    //    var filter = await _bokulousDbService.GetBookAsync();
    //    if (filter.Count == 0 || filter is null)
    //        return NotFound("No books found");

    //    var booksFiltered = filter.Where(x => x.Title.Contains(keyword) || x.Authors.Contains(keyword)).ToList();

    //    if (booksFiltered.Count == 0 || booksFiltered is null)
    //        return NotFound("No books found");

    //    return Ok(booksFiltered);
    //}

    [HttpGet("GetBooksFiltered")]
    public async Task<ActionResult<List<Book>>> GetBookByAdvancedFilter(string title, string author, string category, string language, int priceMin, int priceMax, int yearMin, int yearMax)
    {
        var result = new List<Book>();

        if (language is not null)
        {
            var filter = await _bokulousDbService.GetBookAsync();
           
            var booksFiltered = filter.Where(x => x.Language.Contains(language)).ToList();

            result.AddRange(booksFiltered);
        }

        if (author is not null)
        {
            var filter = await _bokulousDbService.GetBookAsync();

            var booksFiltered = filter.Where(x => x.Authors.Contains(author)).ToList();

            result.AddRange(booksFiltered);
        }
        
        if (title is not null)
        {
            var filter = await _bokulousDbService.GetBookAsync();

            var booksFiltered = filter.Where(x => x.Title.Contains(title)).ToList();

            result.AddRange(booksFiltered);
        }

        if (category is not null)
        {
            var filter = await _bokulousDbService.GetBookAsync();

            var booksFiltered = filter.Where(x => x.Categories.Contains(category)).ToList();

            result.AddRange(booksFiltered);
        }

        if (priceMax != null || priceMin != null)
        {
            var filter = await _bokulousDbService.GetBookAsync();

            var booksFiltered = filter.Where(x => x.Price <= priceMax && x.Price >= priceMin).ToList();

            result.AddRange(booksFiltered);
        }

        if (yearMax != null || yearMin != null)
        {
            var filter = await _bokulousDbService.GetBookAsync();

            var booksFiltered = filter.Where(x => x.Published <= yearMax && x.Published >= yearMin).ToList();

            result.AddRange(booksFiltered);
        }

        return Ok(result);
    }
}