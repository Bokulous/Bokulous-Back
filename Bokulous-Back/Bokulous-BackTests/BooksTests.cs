using Bokulous_Back.Controllers;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Bokulous_BackTests.Data;
using BookStoreApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bokulous_Back.Tests
{
    [Collection("Sequential")]
    public class BooksTests : IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly IBokulousDbService dbService;
        private readonly IBokulousMailService mailService;

        private readonly UserHelpers UserHelpers;
        private readonly AdminController AdminController;
        private readonly UsersController UsersController;
        private readonly BooksController BooksController;
        private readonly TestDbData TestData;

        public BooksTests()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            IOptions<BokulousDatabaseSettings> databaseSettings = Options.Create(configuration.GetSection("BokulousDatabase").Get<BokulousDatabaseSettings>());
            IOptions<BokulousMailSettings> mailSettings = Options.Create(configuration.GetSection("BokulousMailSettings").Get<BokulousMailSettings>());

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
        public async void GetBooksByAuthor_Pass()
        {
            var actionResult = await BooksController.GetBooksByAuthor("Testersson");
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.True(statusCodeResult.StatusCode == 200);
        }

        public void Dispose()
        {
            TestData.RemoveDataFromDb();
        }
    }
}