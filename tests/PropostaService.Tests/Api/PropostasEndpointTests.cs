using FluentAssertions;
using PropostaService.Contracts;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PropostaService.Tests.Api;

public class PropostasEndpointTests : IClassFixture<SqliteWebAppFactory>
{
    private readonly SqliteWebAppFactory _factory;

    public PropostasEndpointTests(SqliteWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task POST_Propostas_DeveRetornar201_ComId()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/propostas", new CreatePropostaDto
        {
            NomeCliente = "Teste",
            Valor = 1200m
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        payload.Should().NotBeNull();
        payload!.Should().ContainKey("id"); 

        var idStr = payload["id"].GetString();
        Guid.TryParse(idStr, out var id).Should().BeTrue();
        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GET_Propostas_DeveRetornar200_ComLista()
    {
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/propostas", new CreatePropostaDto { NomeCliente = "X", Valor = 100m });
        var resp = await client.GetAsync("/propostas");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body.Should().NotBeNull();

        body!.Keys.Should().Contain(k => string.Equals(k, "items", StringComparison.OrdinalIgnoreCase));
        body!.Keys.Should().Contain(k => string.Equals(k, "count", StringComparison.OrdinalIgnoreCase));
    }


    [Fact]
    public async Task PUT_Status_DeveRetornar204_QuandoExiste()
    {
        var client = _factory.CreateClient();

        var create = await client.PostAsJsonAsync("/propostas", new CreatePropostaDto { NomeCliente = "X", Valor = 100m });
        var created = await create.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var id = created!["id"].GetString();

        var put = await client.PutAsJsonAsync($"/propostas/{id}/status", new UpdateStatusDto
        {
            Status = PropostaService.Domain.Entities.PropostaStatus.Aprovada
        });

        put.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_Status_DeveRetornar404_QuandoNaoExiste()
    {
        var client = _factory.CreateClient();
        var put = await client.PutAsJsonAsync($"/propostas/{Guid.NewGuid()}/status", new UpdateStatusDto
        {
            Status = PropostaService.Domain.Entities.PropostaStatus.Aprovada
        });

        put.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Propostas_DeveRetornar400_QuandoPayloadInvalido()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/propostas", new CreatePropostaDto { NomeCliente = "", Valor = -10m });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
