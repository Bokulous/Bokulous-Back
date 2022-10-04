using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Bokulous_BackTests.Data;
using BookStoreApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Xunit;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class UsersTests : IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly IBokulousDbService dbService;
        private readonly IBokulousMailService mailService;

        private readonly UserHelpers UserHelpers;
        private readonly AdminController AdminController;
        private readonly UsersController UsersController;
        private readonly BooksController BooksController;
        private readonly TestDbData TestData;

        public UsersTests()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"appsettings.json", false, false)
                .AddUserSecrets<BokulousDatabaseSettings>()
                .AddUserSecrets<BokulousMailSettings>()
                .AddEnvironmentVariables()
                .Build();

            IOptions<BokulousDatabaseSettings> databaseSettings = Options.Create(configuration.GetSection("BokulousDatabase").Get<BokulousDatabaseSettings>());
            IOptions<BokulousMailSettings> mailSettings = Options.Create(new BokulousMailSettings());

            dbService = new BokulousDbService(databaseSettings);
            mailService = new BokulousMailService(mailSettings);

            UserHelpers = new(dbService);
            AdminController = new(dbService, mailService);
            UsersController = new(dbService, mailService);
            BooksController = new(dbService, mailService);
            TestData = new(dbService);

            TestData.AddDataToDb();
        }

        [Fact()]
        public async void GetUsersReturns200()
        {
            var result = await UsersController.GetUsers();
            var statusCodeResult = result.Result as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Fact()]
        public async void GetUserReturns200()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");
            var result = await UsersController.GetUser(user.Id);
            var statusCodeResult = result.Result as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async void GetUserWhereIdIsNullOrEmptyReturns404(string id)
        {
            var result = await UsersController.GetUser(id);
            var statusCodeResult = result.Result as StatusCodeResult;
            Assert.True(statusCodeResult.StatusCode == 404);
        }

        //INTE FÄRDIG
        [Fact()]
        public async void AddUserReturns200()
        {
            var user = new User()
            {
                Username = "TEST_ADDUSER",
                Mail = "test@test.com",
                Password = "123456"
            };
            var result = await UsersController.AddUser(user);
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        [Fact()]
        public async void AddUserWhereUserIsNullReturns400()
        {
            var user = new User();
            user = null;
            var result = await UsersController.AddUser(user);
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void AddUserWhereUsernameExistsReturns400()
        {
            var user = TestData.Users.FirstOrDefault(x => x.Username == "TEST_USER1");
            var result = await UsersController.AddUser(user);
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void AddUserWhereMailExistsReturns400()
        {
            var user = new User()
            {
                Username = "TEST_USER3",
                Mail = "bla2@bla.com",
                Password = "123456"
            };
            var result = await UsersController.AddUser(user);
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void AddUserWhereUsernameIsNotValidReturns400()
        {
            var user = new User()
            {
                Username = "",
                Mail = "test@testmail.com",
                Password = "123456"
            };
            var result = await UsersController.AddUser(user);
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.True(statusCodeResult.StatusCode == 400);
        }

        [Fact()]
        public async void AddUserWherePasswordIsNotValidExistsReturns400()
        {
            var user = new User()
            {
                Username = "TEST_USER4",
                Mail = "hello@.com",
                Password = ""
            };
            var result = await UsersController.AddUser(user);
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.True(statusCodeResult.StatusCode == 400);
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
            var userDontExist = new User()
            { Id = "111111111111111111111111"};
            var actionResult = await UsersController.ChangePassword(userDontExist.Id, password);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }
        [Fact()]
        public void ForgotPasswordTest()
        {
            //arrange
            var user = TestData.Users.FirstOrDefault(x => x.Mail == "bla1@bla.com");

            //var password = "newpassword123";

            //act
            var response = UsersController.ForgotPassword(user.Mail).Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            //var value = response.Value as User;

            //assert
            //Assert.True(value.Password == user.Password);
            Assert.True(response.StatusCode == 200);
        }

        [Fact()]
        public void ForgotUsernameTest()
        {
            //arrange
            var user = TestData.Users.FirstOrDefault(x => x.Mail == "bla1@bla.com");

            //act
            var response = UsersController.ForgotUsername(user.Mail).Result as Microsoft.AspNetCore.Mvc.ObjectResult;

            //assert
            Assert.True(response.StatusCode == 200);
        }
        public void Dispose()
        {
            TestData.RemoveDataFromDb();
        }
    }
}