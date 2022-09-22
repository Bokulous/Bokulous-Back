using Bokulous_Back.Models;
using Bokulous_Back.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bokulous_BackTests
{
    internal class TestDbData
    {
        BokulousDbService _dbService;

        public List<User> Users { get; set; }
        public List<Book> Books { get; set; }
        public List<Category> Categories { get; set; }

        public TestDbData(BokulousDbService dbService)
        {
            _dbService = dbService;

            Users = new();
            Books = new();
            Categories = new();
        }

        private void AddUsers()
        {
            Users.Add(new User()
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

            Users.Add(new User()
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

            Users.Add(new User()
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
        }

        private void AddBooks()
        {
            Books.Add(new Book()
            {
                ISBN = "12345",
                Title = "TEST",
                Categories = new string[] { "Skräck" },
                Language = "Svenska",
                Authors = new string[] { "Testy Testersson" },
                Published = 2022,
                Weight = 300,
                IsUsed = false,
                InStorage = 5,
                Price = 100,
                Seller = null,
                BookCover = default
            });
        }

        private void AddCategories()
        {
            Categories.Add(new Category()
            {
                Name = "Komedi TEST"
            });
            Categories.Add(new Category()
            {
                Name = "Fakta TEST"
            });
            Categories.Add(new Category()
            {
                Name = "Skräck TEST"
            });
        }

        public void AddDataToDb()
        {
            AddUsers();
            AddBooks();
            AddCategories();

            Users.ForEach(async (user) => await _dbService.CreateUserAsync(user));
            Books.ForEach(async (book) => await _dbService.CreateBookAsync(book));
            Categories.ForEach(async (category) => await _dbService.CreateCategoryAsync(category));

            Thread.Sleep(1000);

            Users = _dbService.GetUserAsync().Result;
            Books = _dbService.GetBookAsync().Result;
            Categories = _dbService.GetCategoryAsync().Result;
        }

        public void RemoveDataFromDb()
        {
            Users = _dbService.GetUserAsync().Result;
            Books = _dbService.GetBookAsync().Result;
            Categories = _dbService.GetCategoryAsync().Result;

            Users.ForEach(async (user) =>
            {
                if (user.Username.Contains("TEST_"))
                {
                    await _dbService.RemoveUserAsync(user.Id);
                }
            });

            Books.ForEach(async (book) =>
            {
                if (book.Title.Contains("TEST"))
                {
                    await _dbService.RemoveBookAsync(book.Id);
                }
            });

            Categories.ForEach(async (category) =>
            {
                if (category.Name.Contains("TEST"))
                {
                    await _dbService.RemoveCategoryAsync(category.Id);
                }
            });
        }
    }
}
