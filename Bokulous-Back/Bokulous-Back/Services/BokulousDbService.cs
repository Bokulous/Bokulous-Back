using Bokulous_Back.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Diagnostics;

namespace Bokulous_Back.Services
{
    public class BokulousDbService : IBokulousDbService
    {
        private readonly IMongoCollection<Book> booksCollection;
        private readonly IMongoCollection<User> usersCollection;
        private readonly IMongoCollection<Category> categoriesCollection;

        [ActivatorUtilitiesConstructor]
        public BokulousDbService(IOptions<BokulousDatabaseSettings> bokulousDatabaseSettings)
        {
            var mongoClient = new MongoClient(bokulousDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(bokulousDatabaseSettings.Value.DatabaseName);

            booksCollection = mongoDatabase.GetCollection<Book>("Books");
            usersCollection = mongoDatabase.GetCollection<User>("Users");
            categoriesCollection = mongoDatabase.GetCollection<Category>("Categories");

            Debug.WriteLine("Using DB: " + bokulousDatabaseSettings.Value.DatabaseName);
        }

        //BOOKS CRUD
        public async Task<List<Book>> GetBookAsync() =>
            await booksCollection.Find(_ => true).ToListAsync();

        public async Task<Book?> GetBookAsync(string id) =>
            await booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateBookAsync(Book newBook) =>
            await booksCollection.InsertOneAsync(newBook);

        public async Task UpdateBookAsync(string id, Book updatedBook) =>
            await booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveBookAsync(string id) =>
            await booksCollection.DeleteOneAsync(x => x.Id == id);

        //USERS CRUD
        public async Task<List<User>> GetUserAsync() =>
            await usersCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetUserAsync(string id) =>
            await usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateUserAsync(User newUser) =>
            await usersCollection.InsertOneAsync(newUser);

        public async Task UpdateUserAsync(string id, User updatedUser) =>
            await usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

        public async Task RemoveUserAsync(string id) =>
            await usersCollection.DeleteOneAsync(x => x.Id == id);

        //CATEGORIES CRUD
        public async Task<List<Category>> GetCategoryAsync() =>
            await categoriesCollection.Find(_ => true).ToListAsync();

        public async Task<Category?> GetCategoryAsync(string id) =>
            await categoriesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateCategoryAsync(Category newCategory) =>
            await categoriesCollection.InsertOneAsync(newCategory);

        public async Task UpdateCategoryAsync(string id, Category updatedCategory) =>
            await categoriesCollection.ReplaceOneAsync(x => x.Id == id, updatedCategory);

        public async Task RemoveCategoryAsync(string id) =>
            await categoriesCollection.DeleteOneAsync(x => x.Id == id);

        //Login
        public async Task<User> LoginAsync(User userLogin) =>
            await usersCollection.Find(o => o.Username.ToLower() == userLogin.Username.ToLower() && o.Password == userLogin.Password).FirstOrDefaultAsync();

        public async Task<User?> GetUserMailAsync(string mail) =>
           await usersCollection.Find(x => x.Mail == mail).FirstOrDefaultAsync();

    }
}