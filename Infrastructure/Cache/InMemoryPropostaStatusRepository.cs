using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;
using ContratacaoService.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;

namespace ContratacaoService.Infrastructure.Cache;

/// <summary>
/// Implementação de cache em memória para status de propostas
/// Não persiste fisicamente no banco - apenas cache runtime
/// </summary>
public class InMemoryPropostaStatusRepository : IPropostaStatusRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemoryPropostaStatusRepository> _logger;
    private const string CACHE_KEY_PREFIX = "proposta_status_";

    public InMemoryPropostaStatusRepository(
        IMemoryCache memoryCache, 
        ILogger<InMemoryPropostaStatusRepository> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task ArmazenarStatusAsync(Guid propostaId, StatusProposta status, string? motivo = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{propostaId}";
        
        var propostaStatus = new PropostaStatus
        {
            PropostaId = propostaId,
            Status = status,
            UltimaAtualizacao = DateTime.UtcNow,
            Motivo = motivo
        };

        // Cache por 1 hora - após isso faz nova consulta HTTP se necessário
        _memoryCache.Set(cacheKey, propostaStatus, TimeSpan.FromHours(1));
        
        _logger.LogDebug("Status armazenado em cache: PropostaId={PropostaId}, Status={Status}, Motivo={Motivo}", 
            propostaId, status, motivo ?? "N/A");

        return Task.CompletedTask;
    }

    public Task<StatusProposta?> ObterStatusAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{propostaId}";
        
        if (_memoryCache.TryGetValue(cacheKey, out PropostaStatus? cached))
        {
            _logger.LogDebug("Status encontrado em cache: PropostaId={PropostaId}, Status={Status}", 
                propostaId, cached!.Status);
            return Task.FromResult<StatusProposta?>(cached.Status);
        }

        _logger.LogDebug("Status não encontrado em cache: PropostaId={PropostaId}", propostaId);
        return Task.FromResult<StatusProposta?>(null);
    }

    public Task<PropostaStatus?> ObterPropostaStatusAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{propostaId}";
        
        if (_memoryCache.TryGetValue(cacheKey, out PropostaStatus? cached))
        {
            _logger.LogDebug("PropostaStatus completo encontrado em cache: PropostaId={PropostaId}", propostaId);
            return Task.FromResult(cached);
        }

        _logger.LogDebug("PropostaStatus não encontrado em cache: PropostaId={PropostaId}", propostaId);
        return Task.FromResult<PropostaStatus?>(null);
    }
}