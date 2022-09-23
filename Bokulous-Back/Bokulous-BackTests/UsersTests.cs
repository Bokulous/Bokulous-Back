using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Bokulous_BackTests;
using BookStoreApi.Controllers;
using Newtonsoft.Json;
using System.Diagnostics;
using Xunit;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class UsersTests : IDisposable
    {

        BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private AdminController AdminController;
        private UsersController UsersController;
        private BooksController BooksController;
        private TestDbData TestData;


        public UsersTests()
        {
            UserHelpers = new(dbService);
            AdminController = new(dbService);
            UsersController = new(dbService);
            BooksController = new(dbService);
            TestData = new(dbService);

            TestData.AddDataToDb();
        }

        [Fact()]
        public async Task ShowProfileTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");

            user.Password = "";

            var result = (await UsersController.ShowProfile(user)).Result as Microsoft.AspNetCore.Mvc.ObjectResult;

            var expected = user;
            var actual = result.Value;

            Assert.Equal(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
        }

        [Fact()]
        public void LoginTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");

            user.Password = "hej123";

            var response = UsersController.Login(user).Result as Microsoft.AspNetCore.Mvc.OkObjectResult;

            Assert.True(response.StatusCode == 200);
        }
        [Fact()]
        public void LoginWrongPasswordTest()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");

            user.Password = "hej125";

            var response = UsersController.Login(user).Result as Microsoft.AspNetCore.Mvc.NotFoundObjectResult;

            Assert.False(response.StatusCode == 200);
        }

        [Fact()]
        public void RegisterTest()
        {
            User user = new User()
            {
                IsActive = true,
                IsAdmin = false,
                IsBlocked = false,
                IsSeller = false,
                Mail = "bla6@bla.com",
                Password = "hej1234",
                Previous_Orders = new List<UserBooks>(),
                Username = "TEST_USER3"
            };
            TestData.Users.Add(user);

            var response = UsersController.Register(user).Result as Microsoft.AspNetCore.Mvc.StatusCodeResult;

            Assert.True(response.StatusCode == 200);
        }

        [Fact()]
        public void EditProfilePass()
        {
            //arrange
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");

            var password = user.Password;

            user.Username = "TEST_UPDATED_USER";
            user.Mail = "updated@mail.com";

            //act
            var response = UsersController.EditProfile(user.Id, user.Username, user.Mail, user.Password).Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var value = response.Value as User;

            //assert
            Assert.True(value.Mail == user.Mail && value.Username == user.Username);
        }

        public void Dispose()
        {
            TestData.RemoveDataFromDb();
        }
    }
}