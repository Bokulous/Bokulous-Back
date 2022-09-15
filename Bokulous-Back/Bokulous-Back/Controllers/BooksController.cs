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

    [HttpGet("GetBooks/{id:length(24)}")]
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
        await _bokulousDbService.CreateBookAsync(newBook);

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

        if(book is null)
            return NotFound();

        if (book.InStorage > 0)
            book.InStorage--;

        await _bokulousDbService.UpdateBookAsync(id, book);

        return Ok();
    }

    // BuyBook: Parameter- User, Book. Return- OK / Fail. Beskrivning- Om bokantal>0 och användaren inte är försäljare eller admin. Method- post.
    // ska man inte kunna vara säljare/admin OCH köpare?! - kolla att du inte kan köpa din egen bok
     [HttpPost("BuyBook")]
    public async Task<IActionResult> BuyBook(BookUser user, Book book)
    {
        var dbBook = await _bokulousDbService.GetBookAsync(book.Id);

        if(book.Seller == user) //???
        {
            return NotFound(); 
        }

        if (book.InStorage > 0)
        {
            book.InStorage--;
        }

        await _bokulousDbService.UpdateBookAsync(book.Id, book);
            

        if (book.InStorage <= 0)
        {
            await _bokulousDbService.RemoveBookAsync(book.Id);
        }

        //skapa order!
            
        return Ok();
    }


    // GetAuthor/ GetBooksByAuthor: Parameter- Keyword. Return- Books[]. Beskrivning- Returnerar en JSON array med lista på böcker som matchar författaren. Method- post.
      [HttpPost("GetBooksByAuthor")]
    public async Task<ActionResult> GetBooksByAuthor()
    {
        var book = await _bokulousDbService.GetBookAsync();

        //...

        return Ok();
    }
}