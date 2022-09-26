using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Bokulous_BackTests;
using BookStoreApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class AdminTests : IDisposable
    {
        private BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private AdminController AdminController;
        private UsersController UsersController;
        private BooksController BooksController;
        private TestDbData TestData;

        public AdminTests()
        {
            UserHelpers = new(dbService);
            AdminController = new(dbService);
            UsersController = new(dbService);
            BooksController = new(dbService);
            TestData = new(dbService);

            TestData.AddDataToDb();
        }

        [Fact()]
        public async Task CheckIsAdminTest()
        {
            var users = dbService.GetUserAsync();
            var user = users.Result.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var expected = true;

            var actual = UserHelpers.CheckIsAdmin(user.Id, user.Password).Result;

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task CheckIsNotAdminTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");

            var expected = false;

            var actual = UserHelpers.CheckIsAdmin(user.Id, user.Password).Result;

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task ChangeUserPasswordTest()
        {
            const string NEW_PASS = "testpass123";

            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            await AdminController.ChangeUserPass(user.Id, NEW_PASS, admin.Id, admin.Password);

            var expected = NEW_PASS;
            var actual = (await dbService.GetUserAsync())
                            .FirstOrDefault(x => x.Id == user.Id).Password;

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task PurgeBookTest()
        {
            var book = TestData.Books.First();
            var admin = TestData.Users.FirstOrDefault(admin => admin.Username == "TEST_ADMIN");

            var response = (await AdminController.PurgeBook(book.Id, admin.Id, admin.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task PurgeEmptyBooksTest()
        {
            var books = TestData.Books.ToList();
            var admin = TestData.Users.FirstOrDefault(admin => admin.Username == "TEST_ADMIN");

            var response = (await AdminController.PurgeEmptyBooks(admin.Id, admin.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task SetAmountTest()
        {
            const int SET_BOOK_INSTORAGE = 5;

            var book = TestData.Books.FirstOrDefault();
            var admin = TestData.Users.FirstOrDefault(admin => admin.Username == "TEST_ADMIN");

            var response = (await AdminController.SetAmount(SET_BOOK_INSTORAGE, book.Id, admin.Id, admin.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task InactivateSellerTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.InactivateUser(user.Id, admin.Id, admin.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task ListUsers()
        {
            var users = TestData.Users.ToList();
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.ListUsers(admin.Id, admin.Password)).Result as ObjectResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task BlockUserTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.BlockUser(user.Id, admin.Id, admin.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task UnBlockUserTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.UnblockUser(user.Id, admin.Id, admin.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task DemoteTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.Demote(user.Id, admin.Id, admin.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task PromoteTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.Promote(user.Id, admin.Id, admin.Password)) as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public async Task BestCustomerTest()
        {
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");
            var customer = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");

            customer.Previous_Orders.Add(new UserBooks { Authors = TestData.Books[0].Authors, Categories = TestData.Books[0].Categories, Id = TestData.Books[0].Id, InStorage = TestData.Books[0].InStorage, ISBN = TestData.Books[0].ISBN, IsUsed = TestData.Books[0].IsUsed, Language = TestData.Books[0].Language, Price = TestData.Books[0].Price, Published = TestData.Books[0].Published, Title = TestData.Books[0].Title, Weight = TestData.Books[0].Weight });
            //customer.Previous_Orders.Add(new UserBooks { Authors = TestData.Books[1].Authors, Categories = TestData.Books[1].Categories, Id = TestData.Books[1].Id, InStorage = TestData.Books[1].InStorage, ISBN = TestData.Books[1].ISBN, IsUsed = TestData.Books[1].IsUsed, Language = TestData.Books[1].Language, Price = TestData.Books[1].Price, Published = TestData.Books[1].Published, Title = TestData.Books[1].Title, Weight = TestData.Books[1].Weight });

            await dbService.UpdateUserAsync(customer.Id, customer);

            var response = (await AdminController.BestCustomer(admin.Id, admin.Password)).Result as ObjectResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        public async Task SoldItemsTest()
        {
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.SoldItems(admin.Id, admin.Password)).Result as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        public async Task SoldItemsYearTest()
        {
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.SoldItems(2022, admin.Id, admin.Password)).Result as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        public async Task SoldItemsMonthTest()
        {
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.SoldItems(2022, 1, admin.Id, admin.Password)).Result as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        public async Task SoldItemsDayTest()
        {
            var admin = TestData.Users.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var response = (await AdminController.SoldItems(2022, 1, 1, admin.Id, admin.Password)).Result as StatusCodeResult;

            var expected = 200;
            var actual = response?.StatusCode ?? throw new Exception("reponse was null");

            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
            TestData.RemoveDataFromDb();
        }
    }
}