﻿using Xunit;
using Bokulous_Back;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Bokulous_Back.Services;
using Bokulous_Back.Helpers;
using Bokulous_Back.Controllers;
using Bokulous_Back.Models;
using System.Diagnostics;

namespace Bokulous_Back.Tests
{
    public class UsersTests
    {
        BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        private UserHelpers UserHelpers;
        private UsersController UsersController;
        public List<User?> TestUsers { get; set; }
        public User? TestAdmin { get; set; }

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

            TestUsers = dbService.GetUserAsync().Result;
        }
        public async Task ShowProfileTest()
        {
            var user = TestUsers.FirstOrDefault(x => x.Username == "TEST_USER1");

            user.Password = null;

            var expected = user;
            var actual = UsersController.ShowProfile(user);

            Assert.Equal(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
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
            });
        }
    }
}