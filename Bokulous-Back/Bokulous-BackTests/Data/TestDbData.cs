using Bokulous_Back.Models;
using Bokulous_Back.Services;

namespace Bokulous_BackTests.Data
{
    internal class TestDbData
    {
        private IBokulousDbService _dbService;

        public List<User> Users { get; set; }
        public List<Book> Books { get; set; }
        public List<Category> Categories { get; set; }
        public Book book { get; set; }

        public TestDbData(IBokulousDbService dbService)
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
                Previous_Orders = new List<UserBooks>(),
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
                Previous_Orders = new List<UserBooks>(),
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
                Previous_Orders = new List<UserBooks>(),
                Username = "TEST_USER2"
            });
        }

        private void AddBooks()
        {
            Books.Add(new Book()
            {
                ISBN = "12345",
                Title = "TEST",
                Categories = new string[] { "Skräck TEST" },
                Language = "Svenska",
                Authors = new string[] { "Testy Testersson" },
                Published = 2022,
                Weight = 300,
                IsUsed = false,
                InStorage = 5,
                Price = 100,
                Seller = new()
                {
                    Username = "Sarah"
                },
                BookCover = default
            });

            Books.Add(new Book()
            {
                ISBN = "555555",
                Title = "TEST 2",
                Categories = new string[] { "Humor TEST" },
                Language = "Svenska",
                Authors = new string[] { "Testy Testersson" },
                Published = 2022,
                Weight = 300,
                IsUsed = false,
                InStorage = 2,
                Price = 100,
                Seller = new()
                {
                    Username = "Sarah"
                },
                BookCover = default
            });

            Books.Add(new Book()
            {
                ISBN = "22222",
                Title = "TEST 3",
                Categories = new string[] { "Fakta TEST" },
                Language = "Svenska",
                Authors = new string[] { "Testy Testersson" },
                Published = 2022,
                Weight = 300,
                IsUsed = false,
                InStorage = 4,
                Price = 100,
                Seller = new()
                {
                    Username = "Sarah"
                },
                BookCover = default
            });

            Books.Add(new Book()
            {
                ISBN = "54321",
                Title = "TEST 4",
                Categories = new string[] { "Deckare TEST" },
                Language = "Svenska",
                Authors = new string[] { "Testy Testersson" },
                Published = 2022,
                Weight = 300,
                IsUsed = false,
                InStorage = 4,
                Price = 100,
                Seller = new()
                {
                    Username = "Sarah"
                },
                BookCover = default
            });

            book = new Book()
            {
                Title = "TEST BOK",
                InStorage = 2
            };

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
            Categories.Add(new Category()
            {
                Name = "Barnförbjudet TEST"
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

            Users = _dbService.GetUserAsync().Result
                .Where(x => x.Username
                    .Contains("TEST_"))
                .ToList();

            Books = _dbService.GetBookAsync().Result
                .Where(x => x.Title
                    .Contains("TEST"))
                .ToList();

            Categories = _dbService.GetCategoryAsync().Result
                .Where(x => x.Name
                    .Contains("TEST"))
                .ToList();
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