using System.Text.Json.Serialization;

namespace PropostaService.Contracts;

public record PropostaCreatedEvent(
    [property: JsonPropertyName("propostaId")] Guid PropostaId,
    [property: JsonPropertyName("nomeProponente")] string NomeProponente,
    [property: JsonPropertyName("valor")] decimal Valor,
    [property: JsonPropertyName("dataCriacao")] DateTime DataCriacao
);
