using Xunit;
using Bokulous_Back.Controllers;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Bokulous_Back.Models;

namespace Bokulous_Back.Tests
{
    public class UsersTests
    {
        private readonly BokulousDbService _bokulousDbService = new ("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");
        private readonly UsersController controller;
        public User userDontExist;
        public UsersTests()
        {
            controller = new UsersController(_bokulousDbService);

            userDontExist = new User
            {
                Id = "123456789123456789123456",
            };
        }

        [Theory]
        [InlineData(null, "123456", StatusCodes.Status404NotFound)]
        [InlineData("", "123456", StatusCodes.Status404NotFound)]
        public async void ChangePasswordWithEmptyOrNoIdReturnsStatusCode404(string id, string password, int expectedResult)
        {
            var actionResult = await controller.ChangePassword(id, password);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData("98374920347019273", "123", StatusCodes.Status400BadRequest)]
        public async void ChangePasswordWithInvalidPasswordReturnsStatusCode400(string id, string password, int expectedResult)
        {
            var actionResult = await controller.ChangePassword(id, password);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData("123456", StatusCodes.Status404NotFound)]
        public async void ChangePasswordWhereUserIsNullReturnsStatusCode400(string password, int expectedResult)
        {
            var actionResult = await controller.ChangePassword(userDontExist.Id, password);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }
    }
}