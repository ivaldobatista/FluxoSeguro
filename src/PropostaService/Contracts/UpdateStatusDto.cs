using PropostaService.Domain.Entities;

namespace PropostaService.Contracts;

public sealed class UpdateStatusDto
{
    public PropostaStatus Status { get; set; }
}
