namespace ContratacaoService.Domain.Entities;

public sealed class Contratacao
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PropostaId { get; private set; }
    public DateTimeOffset DataContratacao { get; private set; }

    private Contratacao() { } // EF
    public Contratacao(Guid propostaId, DateTimeOffset data)
    {
        if (propostaId == Guid.Empty) throw new ArgumentException("PropostaId inválido.", nameof(propostaId));
        PropostaId = propostaId;
        DataContratacao = data;
    }
}
