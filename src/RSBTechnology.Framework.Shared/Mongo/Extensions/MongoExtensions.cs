

using Microsoft.Extensions.DependencyInjection;

namespace RSBTechnology.Framework.Shared.Mongo.Extensions;

public static class MongoExtensions
{
    public static IServiceCollection AddMongoDbContext(this IServiceCollection services, string connectionString, string databaseName)
    {
        services.AddSingleton(new MongoDbContext(connectionString, databaseName));
        return services;
    }
}
