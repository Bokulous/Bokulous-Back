using Bokulous_Back.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Bokulous_Back.Services
{
    public class BokulousDbService
    {
        private readonly IMongoCollection<Book> _booksCollection;
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Category> _categoriesCollection;

        public BokulousDbService(IOptions<BokulousDatabaseSettings> bokulousDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                bokulousDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bokulousDatabaseSettings.Value.DatabaseName);

            _booksCollection = mongoDatabase.GetCollection<Book>("Books");
            _usersCollection = mongoDatabase.GetCollection<User>("Users");
            _categoriesCollection = mongoDatabase.GetCollection<Category>("Categories");
        }

        public BokulousDbService(string connectionString, string dbName)
        {
            var mongoClient = new MongoClient(
                connectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                dbName);

            _booksCollection = mongoDatabase.GetCollection<Book>("Books");
            _usersCollection = mongoDatabase.GetCollection<User>("Users");
            _categoriesCollection = mongoDatabase.GetCollection<Category>("Categories");
        }

        //BOOKS CRUD
        public async Task<List<Book>> GetBookAsync() =>
            await _booksCollection.Find(_ => true).ToListAsync();

        public async Task<Book?> GetBookAsync(string id) =>
            await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateBookAsync(Book newBook) =>
            await _booksCollection.InsertOneAsync(newBook);

        public async Task UpdateBookAsync(string id, Book updatedBook) =>
            await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveBookAsync(string id) =>
            await _booksCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<List<Book>> GetBooksAsyncByAuthor(string keyword) =>   // behöver ligga i service för att komma åt bookscollection
            await _booksCollection.Find(x => x.Authors.Any(y => y.Contains(keyword))).ToListAsync();

        //USERS CRUD
        public async Task<List<User>> GetUserAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetUserAsync(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateUserAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        public async Task UpdateUserAsync(string id, User updatedUser) =>
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

        public async Task RemoveUserAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);

        //CATEGORIES CRUD
        public async Task<List<Category>> GetCategoryAsync() =>
            await _categoriesCollection.Find(_ => true).ToListAsync();

        public async Task<Category?> GetCategoryAsync(string id) =>
            await _categoriesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateCategoryAsync(Category newCategory) =>
            await _categoriesCollection.InsertOneAsync(newCategory);

        public async Task UpdateCategoryAsync(string id, Category updatedCategory) =>
            await _categoriesCollection.ReplaceOneAsync(x => x.Id == id, updatedCategory);

        public async Task RemoveCategoryAsync(string id) =>
            await _categoriesCollection.DeleteOneAsync(x => x.Id == id);
    }
}