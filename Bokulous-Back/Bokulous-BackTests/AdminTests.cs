using Xunit;
using Bokulous_Back;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bokulous_Back.Services;
using Bokulous_Back.Models;
using System.Diagnostics;

namespace Bokulous_Back.Tests
{

    public class AdminTests : IDisposable
    {
        BokulousDbService dbService = new("mongodb+srv://Bokulous:nwQjaj3eVzesn5P9@cluster0.vtut1fa.mongodb.net/test", "Bokulous");

        public List<User?> TestUsers { get; set; }
        public User? TestAdmin { get; set; }
        public User? TestUser { get; set; }

        public AdminTests()
        {
            var users = dbService.GetUserAsync().Result;

            TestAdmin = users.FirstOrDefault(x => x.Username == "TEST_ADMIN");
            TestUser = users.FirstOrDefault(x => x.Username == "TEST_USER");

            if (TestAdmin is not null)
            {
                Debug.WriteLine("Found user: " + TestAdmin?.Username ?? "null. " + "Removing it to create it again.");
                dbService.RemoveUserAsync(TestAdmin.Id);
            }

            if (TestAdmin is not null)
            {
                Debug.WriteLine("Found user: " + TestAdmin?.Username ?? "null. " + "Removing it to create it again.");
                dbService.RemoveUserAsync(TestUser.Id);
            }

            TestAdmin = new()
            {
                IsActive = true,
                IsAdmin = true,
                IsBlocked = false,
                IsSeller = false,
                Mail = "bla1@bla.com",
                Password = "hej123",
                Previous_Orders = new UserBooks[0],
                Username = "TEST_ADMIN"
            };

            TestUser = new()
            {
                IsActive = true,
                IsAdmin = false,
                IsBlocked = false,
                IsSeller = false,
                Mail = "bla2@bla.com",
                Password = "hej123",
                Previous_Orders = new UserBooks[0],
                Username = "TEST_USER"
            };

            dbService.CreateUserAsync(TestUser);
            dbService.CreateUserAsync(TestAdmin);

        }

        [Fact()]
        public void TestMethodTest()
        {
            Assert.True(true, "This test needs an implementati");
        }

        [Fact()]
        public void ChangeUserPassword()
        {
            var users = dbService.GetUserAsync();
            var user = users.Result.FirstOrDefault(x => x.Username == "TEST_USER");

            Debug.WriteLine("Found user: " + user?.Username ?? "null");

            Assert.NotNull(user);
        }

        public void Dispose()
        {
                dbService.RemoveUserAsync(TestAdmin.Id);
                dbService.RemoveUserAsync(TestUser.Id);
        }
    }
}