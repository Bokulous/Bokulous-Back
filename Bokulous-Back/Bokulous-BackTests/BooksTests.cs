using Xunit;
using Bokulous_Back;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bokulous_Back.Controllers;
using Bokulous_Back.Services;
using BookStoreApi.Controllers;
using Bokulous_Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Bokulous_Back.Tests
{
    public class BooksTests
    {
        private readonly BokulousDbService _bokulousDbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");
        private readonly BooksController controller;

        public BooksTests()
        {
            controller = new BooksController(_bokulousDbService);
        }

        [Theory]
        [InlineData("", "C:\\Users\\Desktop\\image.jpg", StatusCodes.Status404NotFound)]
        [InlineData("123456789012345678901234", "", StatusCodes.Status404NotFound)]
        public async void UploadImageWithNoIdOrUserIsNullReturnsStatusCode404(string id, string imagePath, int expectedResult)
        {
            var actionResult = await controller.UploadImage(id, imagePath);
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            Assert.Equal(expectedResult, statusCodeResult.StatusCode);
        }
    }
}