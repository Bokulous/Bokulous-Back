using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using BookStoreApi.Controllers;
using System.Diagnostics;
using Xunit;

namespace Bokulous_Back.Tests
{
    public class BooksTests : IDisposable
    {
        private BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private BooksController BooksController;
        private UsersController UsersController;
        public List<User?> TestUsers { get; set; }
        public User? TestAdmin { get; set; }
        public List<Book?> TestBooks { get; set; }

        public BooksTests()
        {
            UserHelpers = new(dbService);
            UsersController = new(dbService);
            BooksController = new(dbService);
            TestUsers = new();
            TestBooks = new();

            TestUsers.Add(new User()
            {
                IsActive = true,
                IsAdmin = true,
                IsBlocked = false,
                IsSeller = false,
                Mail = "bla1@bla.com",
                Password = "hej123",
                Previous_Orders = new UserBooks[0],
                Username = "TEST_ADMIN"
            });

            TestUsers.Add(new User()
            {
                IsActive = true,
                IsAdmin = false,
                IsBlocked = false,
                IsSeller = false,
                Mail = "bla2@bla.com",
                Password = "hej123",
                Previous_Orders = new UserBooks[0],
                Username = "TEST_USER1"
            });

            TestUsers.Add(new User()
            {
                IsActive = true,
                IsAdmin = false,
                IsBlocked = false,
                IsSeller = false,
                Mail = "bla3@bla.com",
                Password = "hej123",
                Previous_Orders = new UserBooks[0],
                Username = "TEST_USER2"
            });

            TestBooks.Add(new Book()
            {
                ISBN = "12345",
                Title = "TEST",
                Categories = new string[] { "Skräck" },
                Language = "Svenska",
                Authors = new string[] { "Testy Testersson" },
                Published = 2022,
                Weight = 300,
                IsUsed = false,
                InStorage = 5,
                Price = 100,
                Seller = null,
                BookCover = default
            });

            TestUsers.ForEach(async (user) => await dbService.CreateUserAsync(user));
            TestUsers = dbService.GetUserAsync().Result;

            TestBooks.ForEach(async (book) => await dbService.CreateBookAsync(book));
            TestBooks = dbService.GetBookAsync().Result;
        }

        [Fact()]
        public void BuyBook_Pass()
        {
            //arrange
            var buyer = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER1");

            var seller = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER2");

            var book = TestBooks.FirstOrDefault(x => x.Title == "TEST");

            BookUser bookUser = new()
            {
                Id = seller.Id,
                Mail = seller.Mail,
                Username = seller.Username
            };
            book.Seller = bookUser;

            dbService.UpdateBookAsync(book.Id, book);

            //act
            var response = BooksController.BuyBook(book.Id, buyer.Id, buyer.Password).Result as Microsoft.AspNetCore.Mvc.StatusCodeResult;
            //StatusCodeResult = hämtar statuskoden

            //assert
            Assert.True(response.StatusCode == 200);
        }

        [Fact()]
        private void GetBooksByAuthor_Pass()
        {
            //arrange
            var keyword = "Ekwurtzel";

            //act
            var response = BooksController.GetBooksByAuthor(keyword).Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var value = response.Value as List<Book>;
            //ObjectResult = hämtar hela objektet

            //assert
            Assert.True(value.Count > 0);
        }

        public void Dispose()
        {
            TestUsers = dbService.GetUserAsync().Result;

            TestUsers.ForEach(async (user) =>
            {
                if (user.Username == "TEST_ADMIN")
                {
                    await dbService.RemoveUserAsync(user.Id);
                    TestAdmin = null;
                    Debug.WriteLine("Removing admin: " + user?.Username);
                }
                else if (user.Username.Contains("TEST_"))
                {
                    await dbService.RemoveUserAsync(user.Id);
                    Debug.WriteLine("Removing user: " + user?.Username);
                }
            });

            TestBooks = dbService.GetBookAsync().Result;

            TestBooks.ForEach(async (book) =>
            {
                if (book.Title.Contains("TEST"))
                {
                    await dbService.RemoveBookAsync(book.Id);
                }
            });
        }
    }
}