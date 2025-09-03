using ContratacaoService.Application.Ports;

namespace ContratacaoService.Tests.TestDoubles;

public class FakePropostaGateway : IPropostaGateway
{
    private readonly Dictionary<Guid, PropostaStatus?> _map = new();

    public void SetStatus(Guid propostaId, PropostaStatus status) => _map[propostaId] = status;
    public void SetNotFound(Guid propostaId) => _map[propostaId] = null;

    public Task<PropostaStatus?> ObterStatusAsync(Guid propostaId, CancellationToken ct = default)
    {
        if (_map.TryGetValue(propostaId, out var s)) return Task.FromResult(s);
        return Task.FromResult<PropostaStatus?>(null);
    }
}
