using PropostaService.Application.Interfaces;
using PropostaService.Application.Ports;
using PropostaService.Domain.Entities;

namespace PropostaService.Application.UseCases;

public class PropostaUseCases : IPropostaUseCases
{
    private readonly IPropostaRepository _repository;

    public PropostaUseCases(IPropostaRepository repository) => _repository = repository;

    public async Task<Guid> CriarPropostaAsync(string nome, decimal valor, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do cliente é obrigatório.", nameof(nome));
        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));

        var proposta = new Proposta(nome, valor);
        await _repository.AddAsync(proposta, ct);
        return proposta.Id;
    }

    public Task<IReadOnlyList<Proposta>> ListarPropostasAsync(CancellationToken ct = default) =>
        _repository.GetAllAsync(ct);

    public async Task AlterarStatusAsync(Guid id, PropostaStatus status, CancellationToken ct = default)
    {
        var proposta = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Proposta não encontrada.");

        proposta.AlterarStatus(status);
        await _repository.UpdateAsync(proposta, ct);
    }
}
