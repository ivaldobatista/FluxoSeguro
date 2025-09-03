using System.Net.Http.Json;
using ContratacaoService.Application.Ports;
using System.Text.Json;

namespace ContratacaoService.Infrastructure.Gateways;

public sealed class HttpPropostaGateway : IPropostaGateway
{
    private readonly HttpClient _http;
    public HttpPropostaGateway(HttpClient http) => _http = http;

    private sealed record PropostaItem(Guid Id, string NomeCliente, decimal Valor, int Status);
    private sealed record PropostasListDto(IReadOnlyList<PropostaItem> Items, int Count);

    public async Task<PropostaStatus?> ObterStatusAsync(Guid propostaId, CancellationToken ct = default)
    {
        // Simples e robusto: GET /propostas (lista) e filtra pelo Id.
        // (Se você quiser, depois adicionamos GET /propostas/{id} no PropostaService e trocamos aqui.)
        var resp = await _http.GetAsync("/propostas", ct);
        if (!resp.IsSuccessStatusCode) return null;

        var dto = await resp.Content.ReadFromJsonAsync<PropostasListDto>(cancellationToken: ct);
        var item = dto?.Items?.FirstOrDefault(x => x.Id == propostaId);
        if (item is null) return null;

        return item.Status switch
        {
            1 => PropostaStatus.Aprovada,
            2 => PropostaStatus.Rejeitada,
            _ => PropostaStatus.EmAnalise
        };
    }
}
