using Xunit;
using Bokulous_Back;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bokulous_Back.Services;
using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Controllers;
using Newtonsoft.Json;
using BookStoreApi.Controllers;
using System.Diagnostics;

namespace Bokulous_Back.Tests
{
    public class BooksTests
    {
        BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private BooksController BooksController;
        private UsersController UsersController;
        public List<User?> TestUsers { get; set; }
        public User? TestAdmin { get; set; }
        //public List<Book?> TestBook { get; set; }
        

        public BooksTests()
        {
            UserHelpers = new(dbService);
            UsersController = new(dbService);
            //// lägg till test-bok
            //BooksController = new(dbService);

            BooksController = new(dbService);
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

            //// test köpare (bookuser)   //påbörja, måste skapa en collection för bookusers?!?!??! 
            //TestBuyer = new();
            //TestBuyer.Add(new BookUser()
            //{
            //    //id
            //    mail = "testbuyer@bokulous.com",
            //    username = "TEST_BUYER!"
            //});

            ////Test book
            //TestBook = new();

            //TestBook.Add(new Book()
            //{
            //    // id?
            //    ISBN = "12345",
            //    Title= "Sagan om TEST",
            //    Categories = new string[] {"Skräck"},
            //    Language = "Svenska",
            //    Authors = new string[] { "Test Testsson" },
            //    Published = 2022,
            //    Weight = 300,
            //    IsUsed= false,
            //    InStorage= 5, 
            //    Price= 100,
            //    //Seller  
            //    //inget book cover
            //}); 
            

            //Adding test users to database
            TestUsers.ForEach(async (user) => await dbService.CreateUserAsync(user));

            TestUsers = dbService.GetUserAsync().Result;

            ////lägg till test-bok till databasen
            //TestBook.ForEach(async (book) => await dbService.CreateBookAsync(book));

            //TestBook = dbService.GetBookAsync().Result;
        }
        [Fact()]
        public void TestMethodTest()
        {
            Assert.True(true, "This test needs an implementati");
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
                // lägg till testböcker!
            });


            //[Fact()]    // lyckas inte få till en bookuser, ger upp för nu :(
            //void BuyBook_Pass(Book book, BookUser buyer) // testa att det går att skapa en order om alla checks går igenom -> return ok()
            //{
            //    //arrange
            //      // skapa testbok

            //      // skapa test-buyer(bookuser)

            //    //act
            //    var expected = ""; //return ok()
            //    var actual = ""; //retrun ok()

            //    //assert
            //    Assert.Equal(expected, actual);
            //}

            [Fact()]
            void GetBooksByAuthor_Pass()
            {

             //arrange
              

             //act
                //    var expected = ""; //return ok()
                //    var actual = ""; //retrun ok()

             //assert
                //    Assert.Equal(expected, actual);
            }

        }
    } 
}