using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Bokulous_Back.Helpers
{
    public class BookHelpers
    {
        private static IBokulousDbService _bokulousDbService;

        public BookHelpers(IBokulousDbService bokulousDbService)
        {
            _bokulousDbService = bokulousDbService;
        }

        public async Task<ActionResult> RemoveCategoryFromBooks(List<Book> books, Category category)
        {
            if(books is null || books.Count == 0 || category is null)
                return new StatusCodeResult(StatusCodes.Status400BadRequest);

            foreach (var book in books)
            {
                book.Categories = book.Categories.Where(x => x != category.Name).ToArray();
                book.Categories = book.Categories.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (book.Categories.Length == 0)
                    book.Categories = new List<string>(book.Categories) { "Unsorted" }.ToArray();

               await _bokulousDbService.UpdateBookAsync(book.Id, book);
            }

            return new StatusCodeResult(StatusCodes.Status200OK);
        }
    }
}
