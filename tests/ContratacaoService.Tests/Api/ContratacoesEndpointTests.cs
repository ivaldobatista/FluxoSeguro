using ContratacaoService.Application.Ports;
using ContratacaoService.Contracts;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ContratacaoService.Tests.Api;

public class ContratacoesEndpointTests : IClassFixture<SqliteWebAppFactory>
{
    private readonly SqliteWebAppFactory _factory;
    public ContratacoesEndpointTests(SqliteWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task POST_Contratacoes_DeveRetornar201_QuandoAprovada()
    {
        var client = _factory.CreateClient();
        var propostaId = Guid.NewGuid();

        _factory.Gateway.SetStatus(propostaId, PropostaStatus.Aprovada);

        var resp = await client.PostAsJsonAsync("/contratacoes", new CreateContratacaoDto
        {
            PropostaId = propostaId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        payload.Should().ContainKey("id");
        Guid.Parse(payload!["id"]).Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_Contratacoes_DeveRetornar404_QuandoNaoEncontrada()
    {
        var client = _factory.CreateClient();
        var propostaId = Guid.NewGuid();

        _factory.Gateway.SetNotFound(propostaId);

        var resp = await client.PostAsJsonAsync("/contratacoes", new CreateContratacaoDto
        {
            PropostaId = propostaId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Contratacoes_DeveRetornar400_QuandoNaoAprovada()
    {
        var client = _factory.CreateClient();
        var propostaId = Guid.NewGuid();

        _factory.Gateway.SetStatus(propostaId, PropostaStatus.EmAnalise);

        var resp = await client.PostAsJsonAsync("/contratacoes", new CreateContratacaoDto
        {
            PropostaId = propostaId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Contratacoes_DeveRetornar200_ComLista()
    {
        var client = _factory.CreateClient();
        var propostaId = Guid.NewGuid();

        _factory.Gateway.SetStatus(propostaId, PropostaStatus.Aprovada);
        await client.PostAsJsonAsync("/contratacoes", new CreateContratacaoDto { PropostaId = propostaId });

        var resp = await client.GetAsync("/contratacoes");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body.Should().NotBeNull();
        body!.Keys.Should().Contain("items").And.Contain("count");
    }
}
