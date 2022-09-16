using Xunit;
using Bokulous_Back;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Bokulous_Back.Models;

namespace Bokulous_Back.Tests
{
    public class UsersTests
    {
        private readonly BokulousDbService _bokulousDbService;
        private readonly UsersController controller;
        public  User userDontExist;
        public UsersTests()
        {
            controller = new UsersController(_bokulousDbService);

            userDontExist = new User
            {
                Id = "87367492287367",
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

        //[Theory]
        //[InlineData("123456", StatusCodes.Status404NotFound)]
        //public async void ChangePasswordWhereUserIsNullReturnsStatusCode400(string password, int expectedResult)
        //{
        //    var actionResult = await controller.ChangePassword(userDontExist.Id, password);
        //    var statusCodeResult = (IStatusCodeActionResult)actionResult;
        //    Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        //}
    }
}