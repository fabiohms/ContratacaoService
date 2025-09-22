using ContratacaoService.Application.Ports.Inbound;
using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Application.UseCases;
using ContratacaoService.Infrastructure;
using ContratacaoService.Infrastructure.Adapters.Inbound.Messaging;
using ContratacaoService.Infrastructure.Adapters.Outbound.Http;
using ContratacaoService.Infrastructure.Adapters.Outbound.Persistence;
using ContratacaoService.Infrastructure.Cache;
using ContratacaoService.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Memory Cache for PropostaStatus
builder.Services.AddMemoryCache();

// Configuration
builder.Services.Configure<RabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));

// DB (PostgreSQL) - apenas para Contratacoes
var connectionString = builder.Configuration.GetConnectionString("ContratacaoDb") ??
                        "Host=host.docker.internal;Port=5432;Database=contratacao;Username=postgres;Password=postgres";

// Get database configuration
var dbConfig = builder.Configuration.GetSection("Database");
var maxRetryCount = dbConfig.GetValue<int>("MaxRetryCount", 3);
var maxRetryDelaySeconds = dbConfig.GetValue<int>("MaxRetryDelaySeconds", 5);
var connectionTimeoutSeconds = dbConfig.GetValue<int>("ConnectionTimeoutSeconds", 30);

// Add connection retry policy for database
builder.Services.AddDbContext<ContratacaoDbContext>(options => 
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: maxRetryCount,
            maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelaySeconds),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(connectionTimeoutSeconds);
    });
});

// === PORTS AND ADAPTERS CONFIGURATION ===

// Use Cases (Application Core)
builder.Services.AddScoped<IContratacaoUseCase, ContratacaoUseCase>();

// Infrastructure Dependencies (for adapters)
builder.Services.AddScoped<ContratacaoService.Domain.Repositories.IContratacaoRepository, ContratacaoRepository>();
// PropostaStatus agora usa cache em memória, não EF Core
builder.Services.AddScoped<ContratacaoService.Domain.Repositories.IPropostaStatusRepository, InMemoryPropostaStatusRepository>();

// HTTP Client for PropostaService (using built-in resilience in .NET 8)
var baseUrl = builder.Configuration.GetSection("PropostaService:BaseUrl").Get<string>() ?? "http://localhost:5000";
builder.Services.AddHttpClient("PropostaService", client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();

// Outbound Ports (Application -> Infrastructure) - após configurar HttpClient
builder.Services.AddScoped<IContratacaoRepositoryPort, ContratacaoRepositoryAdapter>();
builder.Services.AddScoped<IPropostaStatusRepositoryPort, PropostaStatusRepositoryAdapter>();
builder.Services.AddScoped<IPropostaServicePort, PropostaServiceAdapter>();

// Inbound Adapters
builder.Services.AddHostedService<PropostaEventConsumer>();

var app = builder.Build();

// Migrate / create database (apenas para Contratacoes) with retry logic
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();
    
    var migrationMaxRetries = Math.Max(maxRetryCount, 5); // At least 5 retries for migration
    var delay = TimeSpan.FromSeconds(2);
    
    for (int i = 0; i < migrationMaxRetries; i++)
    {
        try
        {
            logger.LogInformation("Attempting to connect to database and run migrations... (Attempt {Attempt}/{MaxRetries})", i + 1, migrationMaxRetries);
            logger.LogInformation("Using connection string: {ConnectionString}", connectionString.Replace("Password=postgres", "Password=***"));
            
            // Test connection first
            await db.Database.CanConnectAsync();
            logger.LogInformation("Database connection successful!");
            
            // Run migrations
            await db.Database.MigrateAsync();
            
            logger.LogInformation("Database migration completed successfully.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to database or run migrations. Attempt {Attempt}/{MaxRetries}", i + 1, migrationMaxRetries);
            
            if (i == migrationMaxRetries - 1)
            {
                logger.LogError("All database connection attempts failed. Please ensure PostgreSQL is running and accessible.");
                logger.LogError("Troubleshooting tips:");
                logger.LogError("1. If running locally: Check if PostgreSQL is running on localhost:5432");
                logger.LogError("2. If using Docker: Use 'host.docker.internal' instead of 'localhost' in connection string");
                logger.LogError("3. Check firewall settings and PostgreSQL configuration (pg_hba.conf, postgresql.conf)");
                logger.LogError("4. Verify database credentials and that the database 'contratacao' exists");
                
                // Don't throw in production, just log and continue
                if (app.Environment.IsDevelopment())
                {
                    throw;
                }
                else
                {
                    logger.LogWarning("Continuing without database migration in production mode. Database must be migrated manually.");
                }
                break;
            }
            
            logger.LogInformation("Waiting {Delay} seconds before next attempt...", delay.TotalSeconds);
            await Task.Delay(delay);
            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 30)); // Exponential backoff with max 30s
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
