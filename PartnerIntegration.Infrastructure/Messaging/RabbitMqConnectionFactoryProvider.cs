using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PartnerIntegration.Infrastructure.Messaging;

/// <summary>
/// Builds a single, shared RabbitMQ IConnectionFactory from configuration.
/// Registered as a singleton so the underlying TCP connection is reused
/// across requests instead of being re-established on every publish.
/// </summary>
public interface IRabbitMqConnectionProvider
{
    IConnection GetOrCreateConnection();
}

public sealed class RabbitMqConnectionProvider : IRabbitMqConnectionProvider, IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly object _lock = new();
    private IConnection? _connection;

    public RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options)
    {
        var settings = options.Value;

        _connectionFactory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };
    }

    public IConnection GetOrCreateConnection()
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        lock (_lock)
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            _connection?.Dispose();
            _connection = _connectionFactory.CreateConnection();
            return _connection;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
