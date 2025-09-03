namespace PropostaService.Domain.Entities;

public class Proposta
{
    public Guid Id { get; private set; }
    public string NomeProponente { get; private set; }
    public decimal Valor { get; private set; }
    public PropostaStatus Status { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private Proposta() { }

    public Proposta(string nomeProponente, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nomeProponente))
            throw new ArgumentException("Nome do proponente não pode ser vazio.", nameof(nomeProponente));

        if (valor <= 0)
            throw new ArgumentException("Valor da proposta deve ser positivo.", nameof(valor));

        Id = Guid.NewGuid();
        NomeProponente = nomeProponente;
        Valor = valor;
        Status = PropostaStatus.EmAnalise; // Status inicial padrão
        DataCriacao = DateTime.UtcNow;
    }

    public void AlterarStatus(PropostaStatus novoStatus)
    {
        // Regra de negócio: uma vez Aprovada ou Rejeitada, não pode ser alterada.
        if (Status == PropostaStatus.Aprovada || Status == PropostaStatus.Rejeitada)
        {
            throw new InvalidOperationException($"Não é possível alterar o status de uma proposta que já foi '{Status}'.");
        }

        Status = novoStatus;
    }
}

public enum PropostaStatus
{
    EmAnalise,
    Aprovada,
    Rejeitada
}
