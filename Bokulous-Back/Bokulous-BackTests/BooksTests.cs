using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using BookStoreApi.Controllers;
using System.Diagnostics;
using Xunit;
using System.Threading.Tasks;
using Bokulous_BackTests;
using Microsoft.AspNetCore.Mvc;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class BooksTests : IDisposable
    {
        private BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private AdminController AdminController;
        private UsersController UsersController;
        private BooksController BooksController;
        private TestDbData TestData;

        public BooksTests()
        {
            UserHelpers = new(dbService);
            AdminController = new(dbService);
            UsersController = new(dbService);
            BooksController = new(dbService);
            TestData = new(dbService);

            TestData.AddDataToDb();
        }

        [Fact()]
        public async Task BuyBook_Pass()
        {
            //arrange
            var buyer = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");

            var seller = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER2");

            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");

            book.Seller = new()
            {
                Id = seller.Id,
                Mail = seller.Mail,
                Username = seller.Username
            };

            await dbService.UpdateBookAsync(book.Id, book);
            Thread.Sleep(1000);

            //act
            var response = (await BooksController.BuyBook(book.Id, buyer.Id, buyer.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response.StatusCode;
            //StatusCodeResult = hämtar statuskoden

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task GetBooksByAuthor_Pass()
        {
            //arrange
            var keyword = "Ekwurtzel";

            //act
            var response = (await BooksController.GetBooksByAuthor(keyword)) as ObjectResult;
            //var value = response as List<Book>;
            //ObjectResult = hämtar hela objektet

            //assert
            Assert.True(response.StatusCode == 200);
        }

        public void Dispose()
        {
            TestData.RemoveDataFromDb();
        }
    }
}