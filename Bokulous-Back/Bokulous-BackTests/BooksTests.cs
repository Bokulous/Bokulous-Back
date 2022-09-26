using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using BookStoreApi.Controllers;
using Xunit;
using Bokulous_BackTests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class BooksTests : IDisposable
    {
        private BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private BookHelpers BookHelpers;
        private AdminController AdminController;
        private UsersController UsersController;
        private BooksController BooksController;
        private TestDbData TestData;        

        public BooksTests()
        {
            UserHelpers = new(dbService);
            BookHelpers = new(dbService);
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
        public async void GetBooksReturns200()
        {
            var result = await BooksController.GetBooks();
            var statusCodeResult = result.Result as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Fact()]
        public async void GetBookReturns200()
        {
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var result = await BooksController.GetBook(book.Id);
            var statusCodeResult = result.Result as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Theory]
        [InlineData("", StatusCodes.Status400BadRequest)]
        [InlineData(null, StatusCodes.Status400BadRequest)]
        public async void GetBookWhereIdIsNullOrEmptyReturns400(string id, int expectedResult)
        {
            var result = await BooksController.GetBook(id);
            var statusCodeResult = result.Result as StatusCodeResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Fact()]
        public async void GetBookWhereBookDontExistReturns404()
        {
            var book = new Book()
            {
                Id = "111111111111111111111111"
            };
            var result = await BooksController.GetBook(book.Id);
            var statusCodeResult = result.Result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 404);
        }

        [Fact()]
        public async void AddBookReturns201()
        {
            var result = await BooksController.AddBook(TestData.book);
            var statusCodeResult = result as CreatedAtActionResult;
            Assert.True(statusCodeResult.StatusCode == 201);
        }

        [Fact()]
        public async void AddBookToStorageReturns200()
        {
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST 3" && x.Seller.Username == "Sarah");
            var response = await BooksController.AddBook(book);
            var result = response as ObjectResult;
            var obj = (Book)result.Value;
            Assert.Equal(obj.InStorage, 5);
        }

        [Fact()]
        public async void AddBookWhereBookIsNullReturns400()
        {
            var book = new Book();
            book = null;
            var result = await BooksController.AddBook(book);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void AddBookToCategoryWhereBookIsNullReturns400()
        {
            var book = new Book();
            book = null;
            var category = TestData.Categories.FirstOrDefault(x => x.Name == "Skräck TEST");
            var result = await BooksController.AddBookToCategory(book, category);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void AddBookToCategoryWhereCategoryIsNullReturns400()
        {
            var category = new Category();
            category = null;
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var result = await BooksController.AddBookToCategory(book, category);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void AddBookToCategoryReturns200()
        {
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var category = TestData.Categories.FirstOrDefault(x => x.Name == "Fakta TEST");
            var result = await BooksController.AddBookToCategory(book, category);
            var statusCodeResult = result as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Fact()]
        public async void AddBookToCategoryWhereCategoryExistsReturns400()
        {
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var category = TestData.Categories.FirstOrDefault(x => x.Name == "Skräck TEST");
            var result = await BooksController.AddBookToCategory(book, category);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
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

        [Fact()]
        public async void UpdateBookWhereUpdatedBookIsNullReturns400()
        {
            var updatedBook = new Book();
            updatedBook = null;
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var result = await BooksController.UpdateBook(book.Id, updatedBook);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async void UpdateBookWhereIdIsNullOrEmptyReturns400(string id)
        {
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var result = await BooksController.UpdateBook(id, book);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void UpdateBookWhereBookIdDontExistsReturns404()
        {
            string id = "111111111111111111111111";
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var result = await BooksController.UpdateBook(id, book);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 404);
        }

        [Fact()]
        public async void UpdateBookReturns200()
        {
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var result = await BooksController.UpdateBook(book.Id, book);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async void DeleteBookWhereIdIsNullOrEmptyReturns400(string id)
        {
            var result = await BooksController.DeleteBook(id);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void DeleteBookWhereBookIdDontExistsReturns404()
        {
            string id = "111111111111111111111111";
            var result = await BooksController.DeleteBook(id);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 404);
        }

        [Fact()]
        public async void DeleteBookReturns200()
        {
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST 2");
            var result = await BooksController.DeleteBook(book.Id);
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Fact()]
        public async void DeleteBookFromStorageReturns200()
        {
            var book = TestData.Books.FirstOrDefault(x => x.Title == "TEST");
            var response = await BooksController.DeleteBook(book.Id);           
            var result = response as ObjectResult;
            var obj = (Book)result.Value;
            Assert.Equal(obj.InStorage, 4);
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
            var category = TestData.Categories.FirstOrDefault(x => x.Name == "Barnförbjudet TEST");
            var actionResult = await BooksController.UpdateCategory(category, "Barnbok TEST");
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Theory]
        [InlineData(null, StatusCodes.Status400BadRequest)]
        [InlineData("", StatusCodes.Status400BadRequest)]
        public async void UpdateCategoryWhereCategoryNameIsNullOrEmptyReturns400(string categoryName, int expectedResult)
        {
            var category = TestData.Categories.FirstOrDefault(x => x.Name == "Barnförbjudet TEST");
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
            var actionResult = await BooksController.GetBooksByCategory("Skräck TEST");
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

        [Fact()]
        public async void DeleteCategoryFromBooksAndCollection()
        {
            var category = TestData.Categories.FirstOrDefault(x => x.Name == "Barnförbjudet TEST");
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

        [Fact()]
        public async void RemoveCategoryFromBooksWhereListIsEmptyReturns400()
        {
            var emptyList = new List<Book>();
            var category = TestData.Categories.FirstOrDefault(x => x.Name == "Fakta TEST");
            var result = await BookHelpers.RemoveCategoryFromBooks(emptyList, category);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void RemoveCategoryFromBooksWhereListIsNullReturns400()
        {
            List<Book> nullList = new();
            nullList = null;
            var category = TestData.Categories.FirstOrDefault(x => x.Name == "Fakta TEST");
            var result = await BookHelpers.RemoveCategoryFromBooks(nullList, category);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void RemoveCategoryFromBooksWhereCategoryIsNullReturns400()
        {
            Category category = null;
            var result = await BookHelpers.RemoveCategoryFromBooks(TestData.Books, category);
            var statusCodeResult = result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        public void Dispose()
        {
            TestData.RemoveDataFromDb();
        }
    }
}