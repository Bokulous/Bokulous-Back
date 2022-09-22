using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using BookStoreApi.Controllers;
using System.Diagnostics;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using Xunit.Sdk;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class BooksTests : IDisposable
    {
        private BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private BooksController BooksController;
        private UsersController UsersController;
        public List<User?> TestUsers { get; set; }
        public User? TestAdmin { get; set; }
        public List<Book?> TestBooks { get; set; }
        public List<Category?> TestCategories;

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
                Categories = new string[] { "Skräck", "test_pornografi" },
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

            TestBooks.Add(new Book()
            {
                ISBN = "22222",
                Title = "TEST_CATEGORY",
                Categories = new string[] { "test_pornografi" },
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
            TestBooks.ForEach(async (book) => await dbService.CreateBookAsync(book));
            Thread.Sleep(1000);
            TestUsers = dbService.GetUserAsync().Result;
            TestUsers = dbService.GetUserAsync().Result;
            TestBooks = dbService.GetBookAsync().Result;

            TestCategories = new();

            TestCategories.Add(new Category { Name = "test_pornografi" });
            TestCategories.Add(new Category { Name = "test_romantik" });
            TestCategories.Add(new Category { Name = "test_rysare" });
            TestCategories.Add(new Category { Name = "test_humor" });
            TestCategories.Add(new Category { Name = "test_historia" });
            TestCategories.Add(new Category { Name = "test_skönlitteratur" });
            TestCategories.Add(new Category { Name = "test_litteraturvetenskap" });
            TestCategories.Add(new Category { Name = "test_deckare" });
            TestCategories.Add(new Category { Name = "test_barnbok" });

            TestCategories.ForEach(async (category) => await dbService.CreateCategoryAsync(category));
            TestCategories = dbService.GetCategoryAsync().Result;
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

        [Theory]
        [InlineData("", "C:\\Users\\Desktop\\image.jpg", StatusCodes.Status404NotFound)]
        [InlineData("123456789012345678901234", "", StatusCodes.Status404NotFound)]
        public async void UploadImageWithNoIdOrUserIsNullReturnsStatusCode404(string id, string imagePath, int expectedResult)
        {
            var actionResult = await BooksController.UploadImage(id, imagePath);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData("", StatusCodes.Status404NotFound)]
        [InlineData(null, StatusCodes.Status404NotFound)]
        public async void GetCategoriesByKeywordWhereKeywordIsNullOrEmptyReturnsStatusCode404(string keyword, int expectedResult)
        {
            var actionResult = await BooksController.GetCategoriesByKeyword(keyword);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData("", StatusCodes.Status404NotFound)]
        [InlineData(null, StatusCodes.Status404NotFound)]
        [InlineData("w", StatusCodes.Status404NotFound)]
        public async void GetBooksByCategoryWhereKeywordIsNullOrEmptyOrDontExistReturnsStatusCode404(string keyword, int expectedResult)
        {
            var actionResult = await BooksController.GetBooksByCategory(keyword);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData("", StatusCodes.Status400BadRequest)]
        [InlineData(null, StatusCodes.Status400BadRequest)]
        public async void AddCategoryWhereCategoryNameIsNullOrEmptyReturns400(string category, int expectedResult)
        {
            var actionResult = await BooksController.AddCategory(category);
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Fact()]
        public async void UpdateCategoryReturns200()
        {
            var category = TestCategories.FirstOrDefault(x => x.Name == "test_pornografi");
            var actionResult = await BooksController.UpdateCategory(category, "test_barnförbjudet");
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Theory]
        [InlineData(null, StatusCodes.Status400BadRequest)]
        [InlineData("", StatusCodes.Status400BadRequest)]
        public async void UpdateCategoryWhereCategoryNameIsNullOrEmptyReturns400(string categoryName, int expectedResult)
        {
            var category = TestCategories.FirstOrDefault(x => x.Name == "test_pornografi");
            var actionResult = await BooksController.UpdateCategory(category, categoryName);
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Fact()]
        public async void UpdateCategoryWhereCategoryIsNullReturns400()
        {
            var actionResult = await BooksController.UpdateCategory(null, "ny kategori");
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void GetCategoriesReturns200AndList()
        {
            var actionResult = await BooksController.GetCategories();
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Theory]
        [InlineData("", StatusCodes.Status404NotFound)]
        [InlineData(null, StatusCodes.Status404NotFound)]
        public async void GetCategoriesByKeywordWhereKeywordIsNullOrEmptyReturns404(string keyword, int expectedResult)
        {
            var actionResult = await BooksController.GetCategoriesByKeyword(keyword);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Fact()]
        public async void GetBooksByCategoryReturns200AndList()
        {
            var actionResult = await BooksController.GetBooksByCategory("Skräck");
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Theory]
        [InlineData("", StatusCodes.Status404NotFound)]
        [InlineData(null, StatusCodes.Status404NotFound)]
        public async void GetBooksByCategoryWhereKeywordIsNullOrEmptyReturns404(string keyword, int expectedResult)
        {
            var actionResult = await BooksController.GetBooksByCategory(keyword);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Fact()]
        public async void DeleteCategoryWhereCategoryIsNullReturns400()
        {
            Category cat = null;
            var actionResult = await BooksController.DeleteCategory(cat);
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        //tar bort kategori och sätter boken med bara en kategori till "Unsorted"
        [Fact()]
        public async void DeleteCategory()
        {
            var category = TestCategories.FirstOrDefault(x => x.Name == "test_pornografi");
            var actionResult = await BooksController.DeleteCategory(category);
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Fact()]
        public async void DeleteCategoryThatDontExistReturns404()
        {
            var category = new Category()
            {
                Id = "111111111111111111111111",
                Name = "DontExist"
            };
            var actionResult = await BooksController.DeleteCategory(category);
            var statusCodeResult = actionResult as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 404);
        }

        //Testa BookHelper.RemoveCategoryFromBooks

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

            TestCategories = dbService.GetCategoryAsync().Result;

            TestCategories.ForEach(async (category) =>
            {
                if (category.Name.Contains("test"))
                {
                    await dbService.RemoveCategoryAsync(category.Id);
                }
            });
        }
    }
}