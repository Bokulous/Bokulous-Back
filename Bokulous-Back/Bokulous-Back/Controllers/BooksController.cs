using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;

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

    [HttpPut("UpdateBooks/{id:length(24)}")]
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