using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PropostaService.Application.Interfaces;
using PropostaService.Application.Ports;
using PropostaService.Application.UseCases;
using PropostaService.Contracts;
using PropostaService.Infrastructure;
using PropostaService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "propostas.db");

builder.Services.AddDbContext<PropostaDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IPropostaRepository, PropostaRepository>();
builder.Services.AddScoped<IPropostaUseCases, PropostaUseCases>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PropostaService", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
    ctx.Database.Migrate();
}

app.UseSwagger();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PropostaService v1"));
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.MapPost("/propostas",
    async Task<IResult> (IPropostaUseCases uc, CreatePropostaDto body, CancellationToken ct) =>
    {
        if (body is null || string.IsNullOrWhiteSpace(body.NomeCliente) || body.Valor <= 0)
            return Results.BadRequest("Payload inválido. Informe NomeCliente e Valor > 0.");

        var id = await uc.CriarPropostaAsync(body.NomeCliente!, body.Valor, ct);
        return Results.Created($"/propostas/{id}", new { id }); // camelCase no JSON
    })
    .WithTags("Propostas")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.MapGet("/propostas",
    async Task<IResult> (IPropostaUseCases uc, CancellationToken ct) =>
    {
        var list = await uc.ListarPropostasAsync(ct);
        return Results.Ok(new { items = list, count = list.Count }); // camelCase
    })
    .WithTags("Propostas")
    .Produces(StatusCodes.Status200OK);

app.MapPut("/propostas/{id:guid}/status",
    async Task<Results<NoContent, NotFound<string>, BadRequest<string>>>
    (IPropostaUseCases uc, Guid id, UpdateStatusDto body, CancellationToken ct) =>
    {
        if (body is null) return TypedResults.BadRequest("Payload obrigatório.");
        try
        {
            await uc.AlterarStatusAsync(id, body.Status, ct);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound("Proposta não encontrada.");
        }
    })
    .WithTags("Propostas")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest);

app.Run();

public partial class Program { }
