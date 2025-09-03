namespace PropostaService.Contracts;

public sealed class CreatePropostaDto
{
    public string? NomeCliente { get; set; }
    public decimal Valor { get; set; }
}
