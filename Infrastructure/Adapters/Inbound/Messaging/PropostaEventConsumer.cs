using ContratacaoService.Application.Events;
using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.ValueObjects;
using ContratacaoService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ContratacaoService.Infrastructure.Adapters.Inbound.Messaging;

/// <summary>
/// Adapter de entrada para mensagens RabbitMQ
/// </summary>
public class PropostaEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<PropostaEventConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public PropostaEventConsumer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMQOptions> options,
        ILogger<PropostaEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            InitializeRabbitMQ();
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    await ProcessMessage(ea.Body.ToArray(), ea.RoutingKey);
                    _channel?.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem do RabbitMQ");
                    _channel?.BasicNack(ea.DeliveryTag, false, true); // Requeue
                }
            };

            _channel?.BasicConsume(queue: _options.Queue, autoAck: false, consumer: consumer);
            
            _logger.LogInformation("PropostaEventConsumer iniciado. Aguardando mensagens...");
            
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no PropostaEventConsumer");
        }
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange e queue (idempotent)
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(_options.Queue, durable: true, exclusive: false, autoDelete: false);
        
        // Bind queue para receber eventos específicos do PropostaService
        _channel.QueueBind(_options.Queue, _options.Exchange, "PropostaAprovadaEvent");
        _channel.QueueBind(_options.Queue, _options.Exchange, "PropostaRejeitadaEvent");
        
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    private async Task ProcessMessage(byte[] body, string routingKey)
    {
        var message = Encoding.UTF8.GetString(body);
        _logger.LogInformation("Evento recebido [{RoutingKey}]: {Message}", routingKey, message);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var statusRepositoryPort = scope.ServiceProvider.GetRequiredService<IPropostaStatusRepositoryPort>();

            switch (routingKey)
            {
                case "PropostaAprovadaEvent":
                    await ProcessPropostaAprovadaEvent(message, statusRepositoryPort);
                    break;
                case "PropostaRejeitadaEvent":
                    await ProcessPropostaRejeitadaEvent(message, statusRepositoryPort);
                    break;
                default:
                    _logger.LogWarning("Routing key não reconhecida: {RoutingKey}", routingKey);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento {RoutingKey}", routingKey);
            throw;
        }
    }

    private async Task ProcessPropostaAprovadaEvent(string message, IPropostaStatusRepositoryPort repositoryPort)
    {
        var evento = JsonSerializer.Deserialize<PropostaAprovadaEvent>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (evento is null)
        {
            _logger.LogWarning("Não foi possível deserializar PropostaAprovadaEvent: {Message}", message);
            return;
        }

        await repositoryPort.ArmazenarStatusAsync(evento.Id, StatusProposta.Aprovada, motivo: null);
        
        _logger.LogInformation("Proposta {PropostaId} aprovada - Cliente: {Cliente}, Valor: {Valor:C}", 
            evento.Id, evento.Cliente, evento.Valor);
    }

    private async Task ProcessPropostaRejeitadaEvent(string message, IPropostaStatusRepositoryPort repositoryPort)
    {
        var evento = JsonSerializer.Deserialize<PropostaRejeitadaEvent>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (evento is null)
        {
            _logger.LogWarning("Não foi possível deserializar PropostaRejeitadaEvent: {Message}", message);
            return;
        }

        await repositoryPort.ArmazenarStatusAsync(evento.Id, StatusProposta.Reprovada, evento.Motivo);
        
        _logger.LogInformation("Proposta {PropostaId} rejeitada - Cliente: {Cliente}, Valor: {Valor:C}, Motivo: {Motivo}", 
            evento.Id, evento.Cliente, evento.Valor, evento.Motivo);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}