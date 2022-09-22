using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Bokulous_BackTests;
using BookStoreApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Xunit;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class AdminTests : IDisposable
    {
        
        BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

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
            var actual = response.StatusCode;

            Assert.True(true);
        }

        public void Dispose()
        {
            TestData.RemoveDataFromDb();
        }
    }
}