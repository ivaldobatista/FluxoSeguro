using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PropostaService.Application.Interfaces;
using PropostaService.Application.Services;
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
builder.Services.AddScoped<PropostaAppService>();

builder.Services.AddEndpointsApiExplorer(); //????
builder.Services.AddSwaggerGen(); //???

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
    ctx.Database.Migrate();
}

app.UseSwagger(); //??
app.UseSwaggerUI(); //??

app.MapPost("/propostas",
    async Task<IResult> (PropostaAppService service, CreatePropostaDto body, CancellationToken ct) =>
    {
        if (body is null || string.IsNullOrWhiteSpace(body.NomeCliente) || body.Valor <= 0)
            return Results.BadRequest("Payload inválido. Informe NomeCliente e Valor > 0.");

        var id = await service.CriarPropostaAsync(body.NomeCliente!, body.Valor, ct);
        return Results.Created($"/propostas/{id}", new { Id = id });
    });


app.MapGet("/propostas",
    async Task<IResult> (PropostaAppService service, CancellationToken ct) =>
    {
        var list = await service.ListarPropostasAsync(ct);
        return Results.Ok(new { Items = list, Count = list.Count });
    });


app.MapPut("/propostas/{id:guid}/status",
    async Task<Results<NoContent, NotFound<string>, BadRequest<string>>>
    (PropostaAppService service, Guid id, UpdateStatusDto body, CancellationToken ct) =>
    {
        if (body is null) return TypedResults.BadRequest("Payload obrigatório.");
        try
        {
            await service.AlterarStatusAsync(id, body.Status, ct);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound("Proposta não encontrada.");
        }
    });

app.Run();

public partial class Program { }
