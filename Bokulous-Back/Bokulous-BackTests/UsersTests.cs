using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System.Diagnostics;
using Xunit;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class UsersTests : IDisposable
    {
        private BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private UsersController UsersController;
        public List<User?> TestUsers { get; set; }
        public User? TestAdmin { get; set; }
        public User userDontExist;

        public UsersTests()
        {
            UserHelpers = new(dbService);
            UsersController = new(dbService);

            TestUsers = new();

            //Test admin
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

            //Test user
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

            //Test user
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

            //Adding test users to database
            TestUsers.ForEach(async (user) => await dbService.CreateUserAsync(user));
            Thread.Sleep(1000);
            TestUsers = dbService.GetUserAsync().Result;

            userDontExist = new User
            {
                Id = "123456789123456789123456",
            };
        }

        [Fact()]
        public async Task ShowProfileTest()
        {
            var user = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER1");

            user.Password = "";

            var result = (await UsersController.ShowProfile(user)).Result as Microsoft.AspNetCore.Mvc.ObjectResult;

            var expected = user;
            var actual = result.Value;

            Assert.Equal(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
        }

        //FEL VID MERGE?

        //[Fact()]
        //public void LoginTest()
        //{
        //    var user = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER1");

        //    user.Password = "hej123";

        //    var response = UsersController.Login(user).Result as Microsoft.AspNetCore.Mvc.OkObjectResult;

        //    Assert.True(response.StatusCode == 200);
        //}
        //[Fact()]
        //public void LoginWrongPasswordTest()
        //{
        //    var user = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER1");

        //    user.Password = "hej125";

        //    var response = UsersController.Login(user).Result as Microsoft.AspNetCore.Mvc.NotFoundObjectResult;

        //    Assert.False(response.StatusCode == 200);
        //}

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
                Previous_Orders = new UserBooks[0],
                Username = "TEST_USER3"
            };
            TestUsers.Add(user);

            var response = UsersController.Register(user).Result as Microsoft.AspNetCore.Mvc.StatusCodeResult;

            Assert.True(response.StatusCode == 200);
        }

        [Fact()]
        public void EditProfilePass()
        {
            //arrange
            var user = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER1");

            var password = user.Password;

            user.Username = "TEST_UPDATED_USER";
            user.Mail = "updated@mail.com";

            //act
            var response = UsersController.EditProfile(user.Id, user.Username, user.Mail, user.Password).Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var value = response.Value as User;

            //assert
            Assert.True(value.Mail == user.Mail && value.Username == user.Username);
        }

        [Theory]
        [InlineData(null, "123456", StatusCodes.Status404NotFound)]
        [InlineData("", "123456", StatusCodes.Status404NotFound)]
        public async void ChangePasswordWithEmptyOrNoIdReturnsStatusCode404(string id, string password, int expectedResult)
        {
            var actionResult = await UsersController.ChangePassword(id, password);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData("98374920347019273", "123", StatusCodes.Status400BadRequest)]
        public async void ChangePasswordWithInvalidPasswordReturnsStatusCode400(string id, string password, int expectedResult)
        {
            var actionResult = await UsersController.ChangePassword(id, password);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData("123456", StatusCodes.Status404NotFound)]
        public async void ChangePasswordWhereUserIsNullReturnsStatusCode400(string password, int expectedResult)
        {
            var actionResult = await UsersController.ChangePassword(userDontExist.Id, password);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
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

            var TestBooks = dbService.GetBookAsync().Result;
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