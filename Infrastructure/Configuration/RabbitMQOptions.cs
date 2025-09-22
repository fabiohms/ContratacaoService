namespace ContratacaoService.Infrastructure.Configuration;

public class RabbitMQOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "propostas";
    public string Queue { get; set; } = "contratacao.propostas.events";
}