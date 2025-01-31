

using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace RSBTechnology.Framework.Shared.Messaging;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitRabbitMQ(
        this IServiceCollection services,
        string rabbitMqUri,
        string user,
        string password,
        bool enableDelayMessages = false) // Parâmetro para controlar se o delay será usado
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter(); // Formato de nomes dos endpoints

            // Configuração do RabbitMQ
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqUri), h =>
                {
                    h.Username(user);
                    h.Password(password);
                });

                cfg.UseInMemoryOutbox(context);

                // Ativar o delay de mensagens usando o scheduler
                if (enableDelayMessages)
                    cfg.UseDelayedMessageScheduler();

                // Registra automaticamente todos os consumidores que implementam IConsumer
                cfg.ConfigureEndpoints(context);
            });

            // Ativar o delay de mensagens usando 
            if (enableDelayMessages)
                config.AddDelayedMessageScheduler();  // Adiciona o suporte ao plugin de agendamento
        });

        var factory = new ConnectionFactory { HostName = rabbitMqUri };
        return services.AddSingleton(factory);
    }
}
