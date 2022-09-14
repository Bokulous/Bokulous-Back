﻿using Xunit;
using Bokulous_Back;
using Bokulous_Back.Controllers;
using Bokulous_Back.Services;
using Bokulous_Back.Models;
using System.Diagnostics;
using Bokulous_Back.Helpers;

namespace Bokulous_Back.Tests
{

    public class AdminTests : IDisposable
    {
        BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private AdminController AdminController;
        public List<User?> TestUsers { get; set; }
        public User? TestAdmin { get; set; }

        public AdminTests()
        {
            UserHelpers = new(dbService);
            AdminController = new(dbService);

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
            TestUsers.ForEach(user => dbService.CreateUserAsync(user));
        }

        [Fact()]
        public void TestMethodTest()
        {
            Assert.True(true, "This test needs an implementati");
        }

        [Fact()]
        public void CheckIsAdminTest()
        {
            var users = dbService.GetUserAsync();
            var user = users.Result.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            var expected = true;

            var actual = UserHelpers.CheckIsAdmin(user.Id, user.Password).Result;

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public void CheckIsNotAdminTest()
        {
            var user = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER1");

            var expected = false;

            var actual = UserHelpers.CheckIsAdmin(user.Id, user.Password).Result;

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public void ChangeUserPasswordTest()
        {
            const string NEW_PASS = "testpass123";

            var user = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER1");
            var admin = TestUsers.FirstOrDefault(x => x.Username == "TEST_ADMIN");

            AdminController.ChangeUserPass(user.Id, NEW_PASS, admin.Id, admin.Password);

            var expected = NEW_PASS;
            var actual = dbService.GetUserAsync().Result.FirstOrDefault(x => x.Username == user.Username).Password;


            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
            TestUsers.ForEach(user =>
            {
                if (user.Username == "TEST_ADMIN")
                {
                    dbService.RemoveUserAsync(user.Id);
                    TestAdmin = null;
                    Debug.WriteLine("Removing admin: " + user?.Username);
                }
                else if (user.Username.Contains("TEST_"))
                {
                    dbService.RemoveUserAsync(user.Id);
                    Debug.WriteLine("Removing user: " + user?.Username);
                }
            });
        }
    }
}