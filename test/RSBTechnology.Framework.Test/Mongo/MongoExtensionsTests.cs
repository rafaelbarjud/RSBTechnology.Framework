

using Microsoft.Extensions.DependencyInjection;
using RSBTechnology.Framework.Shared.Mongo;
using RSBTechnology.Framework.Shared.Mongo.Extensions;

namespace RSBTechnology.Framework.Test.Mongo;

public class MongoExtensionsTests
{
    [Fact]
    public void AddMongoDbContext_ShouldRegister_MongoDbContext_AsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        string connectionString = "mongodb://localhost:27017";
        string databaseName = "TestDatabase";

        // Act
        services.AddMongoDbContext(connectionString, databaseName);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var registeredService = services.FirstOrDefault(s => s.ServiceType == typeof(MongoDbContext));
        Assert.NotNull(registeredService);
        Assert.Equal(ServiceLifetime.Singleton, registeredService.Lifetime);

        var resolvedService = serviceProvider.GetService<MongoDbContext>();
        Assert.NotNull(resolvedService);
    }
}