using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Skolyn.Platform.DicomIngestion.Application.Interfaces;
using Skolyn.Platform.DicomIngestion.Application.Models;
using System.Text.Json;

public class RabbitMqService : IMessageQueueService
{
    private readonly MessageQueueSettings _settings;

    public RabbitMqService(IOptions<MessageQueueSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
            throw new ArgumentNullException(nameof(_settings.ConnectionString), "Connection string is missing in configuration.");
    }

    public async Task PublishStudyForProcessingAsync(DicomStudyMessage message, CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_settings.ConnectionString) };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(null, cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: cancellationToken
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(
            exchange: _settings.ExchangeName,
            routingKey: "",
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body,
            cancellationToken: cancellationToken
        );
    }
}
