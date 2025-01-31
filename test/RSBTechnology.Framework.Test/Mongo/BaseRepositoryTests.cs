
using MongoDB.Driver;
using Moq;
using RSBTechnology.Framework.Shared.Mongo.Repository;
using RSBTechnology.Framework.Shared.Mongo;
using System.Linq.Expressions;

namespace RSBTechnology.Framework.Test.Mongo;

public class BaseRepositoryTests
{
    private readonly Mock<IMongoCollection<TestEntity>> _mockCollection;
    private readonly BaseRepository<TestEntity> _repository;

    public BaseRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<TestEntity>>();

        // Criar um mock manual para MongoDbContext
        var mockContext = new Mock<MongoDbContext>("mongodb://localhost:27017", "TestDB");

        // Configurar para retornar a coleção mockada
        mockContext.Setup(c => c.GetCollection<TestEntity>(It.IsAny<string>()))
                   .Returns(_mockCollection.Object);

        _repository = new BaseRepository<TestEntity>(mockContext.Object, "testCollection");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Criar uma lista de entidades de teste
        var entities = new List<TestEntity> { new() { Id = "1", Name = "Test 1" } };

        // Mock do cursor assíncrono do MongoDB
        var mockCursor = new Mock<IAsyncCursor<TestEntity>>();
        mockCursor.Setup(_ => _.Current).Returns(entities);
        mockCursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)  // Primeira chamada retorna true (há dados)
            .Returns(false); // Segunda chamada retorna false (fim dos dados)

        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)  // Primeira chamada retorna true (há dados)
            .ReturnsAsync(false); // Segunda chamada retorna false (fim dos dados)

        _mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<TestEntity>>(),
                It.IsAny<FindOptions<TestEntity, TestEntity>>(),
                default))
            .ReturnsAsync(mockCursor.Object);

        // Executar o método do repositório
        var result = await _repository.GetAllAsync();

        // Validar se a coleção não está vazia
        Assert.NotNull(result);
        Assert.Single(result); // Deve conter exatamente um elemento
        Assert.Equal("Test 1", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        var entity = new TestEntity { Id = "1", Name = "Test Entity" };

        var mockCursor = new Mock<IAsyncCursor<TestEntity>>();
        mockCursor.Setup(_ => _.Current).Returns(new List<TestEntity> { entity });
        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<TestEntity>>(),
                It.IsAny<FindOptions<TestEntity, TestEntity>>(),
                default))
            .ReturnsAsync(mockCursor.Object);

        var result = await _repository.GetByIdAsync("1");

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("Test Entity", result.Name);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        var entities = new List<TestEntity>
    {
        new() { Id = "1", Name = "Test 1" },
        new() { Id = "2", Name = "Test 2" }
    };

        var mockCursor = new Mock<IAsyncCursor<TestEntity>>();
        mockCursor.Setup(_ => _.Current).Returns(entities);
        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<TestEntity>>(), // Aqui está a correção!
                It.IsAny<FindOptions<TestEntity, TestEntity>>(),
                default))
            .ReturnsAsync(mockCursor.Object);

        var result = await _repository.FindAsync(e => e.Name.Contains("Test"));

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddAsync_ShouldInsertEntity()
    {
        var entity = new TestEntity { Id = "3", Name = "New Entity" };

        _mockCollection.Setup(c => c.InsertOneAsync(entity, null, default))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

        await _repository.AddAsync(entity);

        _mockCollection.Verify(c => c.InsertOneAsync(entity, null, default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReplaceEntity()
    {
        var entity = new TestEntity { Id = "4", Name = "Updated Entity" };

        _mockCollection.Setup(c => c.ReplaceOneAsync(
                                It.IsAny<FilterDefinition<TestEntity>>(),
                                entity,
                                It.IsAny<ReplaceOptions>(),
                                default))
                       .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, entity.Id))
                       .Verifiable();

        await _repository.UpdateAsync(entity.Id, entity);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<TestEntity>>(),
            entity,
            It.IsAny<ReplaceOptions>(),
            default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        _mockCollection.Setup(c => c.DeleteOneAsync(
                                It.IsAny<FilterDefinition<TestEntity>>(),
                                default))
                       .ReturnsAsync(new DeleteResult.Acknowledged(1))
                       .Verifiable();

        await _repository.DeleteAsync("5");

        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<TestEntity>>(),
            default), Times.Once);
    }
}

public class TestEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
}