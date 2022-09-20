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

    //// BuyBook: Parameter- User, Book. Return- OK / Fail. Beskrivning- Om bokantal>0, säljare kan inte köpa sina egna böcker. Method- post.
    [HttpPost("BuyBook/{bookId:length(24)}")]
    public async Task<IActionResult> BuyBook(string bookId, BookUser buyer)  
    {
        var book = await _bokulousDbService.GetBookAsync(bookId);

        if (book.Seller.Id == buyer.Id)
        {
            return BadRequest("Seller = Buyer");
        }

        if (Request.Headers.TryGetValue("token", out var t)) // kollar om det finns en säkerhets-token -> user är authorized(token skapas i login). Hämtar token ur header. TODO: Ge token till service-> return true/false
        {
            if (book.InStorage > 0)
            {
                book.InStorage--;
            }

            await _bokulousDbService.UpdateBookAsync(book.Id, book); // uppdaterar bokobjektet i db

            if (book.InStorage <= 0) // purgebook- annan endpoint?
            {
                await _bokulousDbService.RemoveBookAsync(book.Id);
            }

            // skapa order
            var newOrder = new Order() {
                Books = new List<Book>() { book },
                Buyer = buyer,
                BuyerAdress = "", // behövs som property i user!
                BookWeight = book.Weight,
                ShippingCost = book.Weight * 0.05, // exempel på portoberäkning. bok på 300g kostar 15kr i porto. flytta portberäkning till hjälpklass?
                TotalBookCost = book.Price,
                TotalOrderCost = book.Price + book.Weight * 0.05
            };
            //TODO
            // https://www.c-sharpcorner.com/article/sending-email-using-c-sharp/ (?)
            // maila orderbekräftelse till köpare
            // ev. maila till säljare av begagnad bok
            // ev. maila plocklista till admin 
            await _bokulousDbService.CreateOrderAsync(newOrder);

            return Ok();
        }
        else return Unauthorized("Not allowed."); //köparens token stämmer inte -> risk för hackattack
    }


    // GetAuthor/ GetBooksByAuthor: Parameter- Keyword. Return- Books[]. Beskrivning- Returnerar en JSON array med lista på böcker som matchar författaren. Method- post.
    // har skrivit metoden i service för att inte behöva dependency injecta bookcollection 
    [HttpPost("GetBooksByAuthor/{id:length(24)}")]
    public async Task<ActionResult> GetBooksByAuthor(string id)
    {
        var books = await _bokulousDbService.GetBooksAsyncByAuthor(id);

        return Ok(books);
    }
}