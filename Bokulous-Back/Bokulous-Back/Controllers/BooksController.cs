using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BooksService _booksService;

    public BooksController(BooksService booksService) =>
        _booksService = booksService;

    [HttpGet("GetBooks")]
    public async Task<List<Book>> GetBooks() =>
        await _booksService.GetAsync();

    [HttpGet("GetBooks/{id:length(24)}")]
    public async Task<ActionResult<Book>> GetBook(string id)
    {
        var book = await _booksService.GetAsync(id);

        if (book is null)
            return NotFound();

        return Ok(book);
    }

    [HttpPost("AddBooks")]
    public async Task<IActionResult> AddBook(Book newBook)
    {
        await _booksService.CreateAsync(newBook);

        return CreatedAtAction(nameof(AddBook), new { id = newBook.Id }, newBook);
    }

    [HttpPut("UpdateBooks/{id:length(24)}")]
    public async Task<IActionResult> UpdateBook(string id, Book updatedBook)
    {
        var book = await _booksService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedBook.Id = book.Id;

        await _booksService.UpdateAsync(id, updatedBook);

        return Ok();
    }

    [HttpPut("DeleteBook/{id:length(24)}")]
    public async Task<IActionResult> DeleteBook(string id)
    {
        var book = await _booksService.GetAsync(id);

        if(book is null)
            return NotFound();

        if (book.InStorage > 0)
            book.InStorage--;

        await _booksService.UpdateAsync(id, book);

        return Ok();
    }
}