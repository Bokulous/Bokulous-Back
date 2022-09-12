using Bokulous_Back.Interfaces;
using Bokulous_Back.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Bokulous_Back.Services
{
    public class BokulousService<T> where T: IItem
    {
        private readonly IMongoCollection<T> _Collection;

        public BokulousService(IOptions<BokulousDatabaseSettings> bokulousDatabaseSettings, string collection)
        {
            var mongoClient = new MongoClient(
                bokulousDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bokulousDatabaseSettings.Value.DatabaseName);

            _Collection = mongoDatabase.GetCollection<T>(collection);
        }

        public async Task<List<T>> GetAsync() =>
            await _Collection.Find(_ => true).ToListAsync();

        public async Task<T?> GetAsync(string id) =>
            await _Collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(T newItem) =>
            await _Collection.InsertOneAsync(newItem);

        public async Task UpdateAsync(string id, T updatedItem) =>
            await _Collection.ReplaceOneAsync(x => x.Id == id, updatedItem);

        public async Task RemoveAsync(string id) =>
            await _Collection.DeleteOneAsync(x => x.Id == id);
    }
}
