using ContratacaoService.Application.Interfaces;
using ContratacaoService.Application.Ports;
using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Application.UseCases;

public sealed class ContratacaoUseCases : IContratacaoUseCases
{
    private readonly IContratacaoRepository _repo;
    private readonly IPropostaGateway _proposta;

    public ContratacaoUseCases(IContratacaoRepository repo, IPropostaGateway proposta)
    {
        _repo = repo;
        _proposta = proposta;
    }

    public async Task<Guid> ContratarAsync(Guid propostaId, CancellationToken ct = default)
    {
        if (propostaId == Guid.Empty) throw new ArgumentException("PropostaId inválido.", nameof(propostaId));

        var status = await _proposta.ObterStatusAsync(propostaId, ct);
        if (status is null) throw new InvalidOperationException("Proposta não encontrada.");
        if (status != PropostaStatus.Aprovada)
            throw new InvalidOperationException("Proposta não está Aprovada.");

        var c = new Contratacao(propostaId, DateTimeOffset.UtcNow);
        await _repo.AddAsync(c, ct);
        return c.Id;
    }

    public Task<IReadOnlyList<Contratacao>> ListarAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);
}
