using ContratacaoService.Application.Interfaces;
using ContratacaoService.Application.Ports;
using ContratacaoService.Application.UseCases;
using ContratacaoService.Contracts;
using ContratacaoService.Infrastructure;
using ContratacaoService.Infrastructure.Gateways;
using ContratacaoService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "contratacoes.db");
builder.Services.AddDbContext<ContratacaoDbContext>(opt =>
opt.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IContratacaoRepository, ContratacaoRepository>();
builder.Services.AddScoped<IContratacaoUseCases, ContratacaoUseCases>();

var baseUrl = builder.Configuration["PropostaService:BaseUrl"] ?? "http://localhost:5024";
builder.Services.AddHttpClient<IPropostaGateway, HttpPropostaGateway>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();
    ctx.Database.Migrate();
}

app.UseSwagger();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"));

var contratacoes = app.MapGroup("/contratacoes").WithTags("Contratações");

contratacoes.MapPost("/",
    async Task<IResult>
    (IContratacaoUseCases uc, CreateContratacaoDto body, CancellationToken ct) =>
    {
        if (body is null || body.PropostaId == Guid.Empty)
            return Results.BadRequest("Payload inválido.");

        try
        {
            var id = await uc.ContratarAsync(body.PropostaId, ct);
            return Results.Created($"/contratacoes/{id}", new { id }); // camelCase
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("não encontrada"))
        {
            return Results.NotFound("Proposta não encontrada.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("não está Aprovada"))
        {
            return Results.BadRequest("Proposta não está Aprovada.");
        }
    });

contratacoes.MapGet("/",
    async Task<IResult> (IContratacaoUseCases uc, CancellationToken ct) =>
    {
        var list = await uc.ListarAsync(ct);
        return Results.Ok(new { items = list, count = list.Count }); // camelCase
    });

app.Run();
public partial class Program { }
