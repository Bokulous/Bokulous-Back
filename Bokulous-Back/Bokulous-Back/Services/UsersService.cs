using Bokulous_Back.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Bokulous_Back.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _UsersCollection;

        public UsersService(IOptions<UsersDatabaseSettings> bokulousDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                bokulousDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bokulousDatabaseSettings.Value.DatabaseName);

            _UsersCollection = mongoDatabase.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetAsync() =>
            await _UsersCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetAsync(string id) =>
            await _UsersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(User newUser) =>
            await _UsersCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, User updatedUser) =>
            await _UsersCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _UsersCollection.DeleteOneAsync(x => x.Id == id);
    }
}
