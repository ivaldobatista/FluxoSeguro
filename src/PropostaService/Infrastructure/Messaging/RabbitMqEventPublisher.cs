using Microsoft.Extensions.Options;
using PropostaService.Application.Ports;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PropostaService.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "propostas.events";
    public string ExchangeType { get; set; } = "topic";
}

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqOptions _opt;

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        _opt = options.Value;

        var factory = new RabbitMQ.Client.ConnectionFactory
        {
            HostName = _opt.HostName,
            Port = _opt.Port,
            UserName = _opt.UserName,
            Password = _opt.Password,
            VirtualHost = _opt.VirtualHost,
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _opt.Exchange,
                                 type: _opt.ExchangeType,
                                 durable: true,
                                 autoDelete: false);

        _logger.LogInformation("RabbitMQ publisher conectado em {Host}:{Port}, exchange={Exchange}",
            _opt.HostName, _opt.Port, _opt.Exchange);
    }

    public Task PublishAsync<T>(string routingKey, T message, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var body = Encoding.UTF8.GetBytes(json);

            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;

            _channel.BasicPublish(exchange: _opt.Exchange,
                                  routingKey: routingKey,
                                  basicProperties: props,
                                  body: body);

            _logger.LogInformation("Evento publicado: rk={RoutingKey}, size={Length}", routingKey, body.Length);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao publicar evento rk={RoutingKey}", routingKey);
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try { _channel?.Close(); } catch {  }
        try { _connection?.Close(); } catch {  }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
