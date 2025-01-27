

using MongoDB.Driver;
using System.Linq.Expressions;

namespace RSBTechnology.Framework.Shared.Mongo.Repository;

public class BaseRepository<T>(MongoDbContext context, string collectionName) : IRepository<T> where T : class
{

    private readonly IMongoCollection<T> _collection = context.GetCollection<T>(collectionName);

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<T> GetByIdAsync(string id) =>
        await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter) =>
        await _collection.Find(filter).ToListAsync();

    public async Task AddAsync(T entity) =>
        await _collection.InsertOneAsync(entity);

    public async Task UpdateAsync(string id, T entity) =>
        await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);

    public async Task DeleteAsync(string id) =>
        await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
}
