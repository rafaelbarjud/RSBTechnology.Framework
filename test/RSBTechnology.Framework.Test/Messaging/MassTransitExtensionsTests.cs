using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RSBTechnology.Framework.Shared.Messaging;

namespace RSBTechnology.Framework.Test.Messaging;

public class MassTransitExtensionsTests
{
    [Fact]
    public void AddMassTransitRabbitMQ_ShouldRegister_ConnectionFactory_AsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var rabbitMqUri = "rabbitmq://localhost";
        var user = "guest";
        var password = "guest";
        var enableDelayMessages = true;

        // Act
        services.AddMassTransitRabbitMQ(rabbitMqUri, user, password, enableDelayMessages);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var registeredService = services.FirstOrDefault(s => s.ServiceType == typeof(ConnectionFactory));
        Assert.NotNull(registeredService);
        Assert.Equal(ServiceLifetime.Singleton, registeredService.Lifetime);

        var resolvedService = serviceProvider.GetService<ConnectionFactory>();
        Assert.NotNull(resolvedService);
        Assert.Equal(rabbitMqUri, resolvedService.HostName);
    }
}

