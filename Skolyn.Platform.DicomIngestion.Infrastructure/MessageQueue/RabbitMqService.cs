using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Skolyn.Platform.DicomIngestion.Application.Interfaces;
using Skolyn.Platform.DicomIngestion.Application.Models;

public class RabbitMqService : IMessageQueueService
{
    private readonly string _hostName;
    private readonly string _exchangeName;
    private readonly string _connectionString;

    public RabbitMqService(IConfiguration configuration)
    {
        _hostName = configuration["MessageQueue:HostName"] ?? "localhost";
        _exchangeName = configuration["MessageQueue:ExchangeName"] ?? "dicom.studies.exchange";
        _connectionString = configuration["MessageQueue:ConnectionString"]
            ?? throw new ArgumentNullException("MessageQueue:ConnectionString is missing in configuration.");
    }

    public async Task PublishStudyForProcessingAsync(DicomStudyMessage message, CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(null, cancellationToken);

        // Use the IChannel interface directly instead of casting to IModel
        await channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: cancellationToken
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: "",
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body,
            cancellationToken: cancellationToken
        );
    }
}
