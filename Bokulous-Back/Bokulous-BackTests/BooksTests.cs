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
            TestData.RemoveDataFromDb();
        }
    }
}