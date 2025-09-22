using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.ValueObjects;
using ContratacaoService.Infrastructure.DTOs;
using ContratacaoService.Domain.Repositories;
using System.Text.Json;

namespace ContratacaoService.Infrastructure.Adapters.Outbound.Http;

/// <summary>
/// Adapter de saída para comunicação HTTP com PropostaService
/// </summary>
public class PropostaServiceAdapter : IPropostaServicePort
{
    private readonly HttpClient _httpClient;
    private readonly IPropostaStatusRepository _statusRepository;
    private readonly JsonSerializerOptions _jsonOptions;

    public PropostaServiceAdapter(IHttpClientFactory httpClientFactory, IPropostaStatusRepository statusRepository)
    {
        _httpClient = httpClientFactory.CreateClient("PropostaService");
        _statusRepository = statusRepository;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<StatusProposta?> ObterStatusAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        // Primeiro tenta buscar no cache local (atualizado via RabbitMQ)
        var statusLocal = await _statusRepository.ObterStatusAsync(propostaId, cancellationToken);
        if (statusLocal.HasValue)
        {
            return statusLocal.Value;
        }

        // Se não encontrar localmente, faz fallback para HTTP (para backward compatibility)
        try
        {
            var response = await _httpClient.GetAsync($"/api/Propostas/{propostaId}", cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            
            // Deserializar usando o DTO tipado
            var propostaJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var proposta = JsonSerializer.Deserialize<PropostaResponse>(propostaJson, _jsonOptions);
            
            if (proposta != null)
            {
                // Armazena no cache local informação obtida via HTTP (para evitar futuras consultas)
                await _statusRepository.ArmazenarStatusAsync(propostaId, proposta.Status, proposta.Motivo, cancellationToken);
                return proposta.Status;
            }
            
            return null;
        }
        catch (HttpRequestException)
        {
            // Log do erro seria ideal aqui
            return null;
        }
        catch (JsonException)
        {
            // Log do erro seria ideal aqui
            return null;
        }
    }
}