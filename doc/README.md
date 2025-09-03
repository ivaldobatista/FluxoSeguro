
# FluxoSeguro — Design & Operating Model (doc)

> Este documento aprofunda a **arquitetura**, **contratos**, **decisões** e **boas práticas** usadas no monorepo.

## Índice

- [Arquitetura Hexagonal](#arquitetura-hexagonal)
- [Camadas e Responsabilidades](#camadas-e-responsabilidades)
- [Contratos & DTOs](#contratos--dtos)
- [Persistência & Migrations](#persistência--migrations)
- [Integração entre Serviços](#integração-entre-serviços)
- [Test Strategy](#test-strategy)
- [Observabilidade & Swagger](#observabilidade--swagger)
- [Segurança (placeholder)](#segurança-placeholder)
- [Roadmap / Próximos Passos](#roadmap--próximos-passos)
- [ADRs](#adrs)

## Arquitetura Hexagonal

- **Input Ports**: Interfaces de caso de uso consumidas por drivers (HTTP, workers).  
  - `IPropostaUseCases`, `IContratacaoUseCases`
- **Output Ports**: Dependências para fora do core (DB/HTTP/filas).  
  - `IPropostaRepository`, `IContratacaoRepository`, `IPropostaGateway`
- **Adapters**:
  - **Driving (entrada)**: Minimal APIs (Swagger/OpenAPI), validador de payload.
  - **Driven (saída)**: EF Core Repositories; `HttpPropostaGateway`.

### Diagrama de alto nível

```mermaid

flowchart TD

Driver[HTTP (Minimal API)] --> IP[I* Ports (UseCases)]
IP --> Domain[Domínio]
IP --> OP[O* Ports (Repo/Gateway)]
OP --> EF[EF Core]
OP --> HTTP[HttpClient -> PropostaService]

```

## Camadas e Responsabilidades

**Domain**

* Entidades ricas com invariantes:

  * `Proposta` (status: EmAnalise/Aprovada/Rejeitada)
  * `Contratacao` (PropostaId, DataContratacao)
* Sem dependência de framework.

**Application**

* **Use Cases** orquestram regras de negócio.
* Validam inputs e chamam **Output Ports**.
* Ex.: `ContratarAsync(propostaId)` → consulta status via `IPropostaGateway` → persiste.

**Infrastructure**

* **EF Core** (`DbContext`, `Repository`) e mapeamentos.
* **HttpPropostaGateway** com `HttpClient`/`BaseAddress` configurável.

**API (Minimal)**

* Endpoints finos.
* Retornos camelCase (`{ id }`, `{ items, count }`).
* **Typed Results** onde útil (204/404/400); `IResult` quando usamos tipos anônimos.

## Contratos & DTOs

**PropostaService**

* `POST /propostas` → `{ "id": "<guid>" }`
* `GET /propostas` → `{ "items": [...], "count": N }`
* `PUT /propostas/{id}/status`

  * Body numérico: `{ "status": 1 }` (0=EmAnalise, 1=Aprovada, 2=Rejeitada)

> Se preferir **enums como string**, adicione `JsonStringEnumConverter` nos `HttpJsonOptions` da API.

**ContratacaoService**

* `POST /contratacoes` → `{ "id": "<guid>" }`
* `GET /contratacoes` → `{ "items": [...], "count": N }`

## Persistência & Migrations

* Banco: **SQLite** local (arquivo em `src/*/data/*.db` em dev).
* Migrations executadas no **startup** (`ctx.Database.Migrate()`), inclusive em containers.
* **ContratacaoService**: `DateTimeOffset` mapeado como **UnixTime (INTEGER)** com `HasConversion` para suportar `ORDER BY` no SQLite.

**Boas práticas**

* Nunca misture `EnsureCreated()` com `Migrate()` no mesmo DB/boot.
* Testes de integração: usar **SQLite in-memory** + **Migrate** (idempotente).
* Em produção real → trocar por Postgres/SQL Server sem tocar nos casos de uso (apenas adapters).

## Integração entre Serviços

* **ContratacaoService → PropostaService** via `IPropostaGateway` (HttpClient).
* Config via `PropostaService__BaseUrl`.
* Em Compose/K8s, use **service discovery** (`http://proposta:8080`).

**Evoluções possíveis**

* Endpoint dedicado `GET /propostas/{id}` em PropostaService para otimizar lookup.
* Retry/polly para resilência do gateway.
* Event-driven: publicar “PropostaAprovada” e consumir no ContratacaoService (bônus com mensageria).

## Test Strategy

* **Domain**: testes puros (sem frameworks).
* **Application**: SQLite in-memory + dublês (FakePropostaGateway).
* **API (integração)**: `WebApplicationFactory<Program>`, override de DI:

  * Troca `DbContext` por SQLite in-memory compartilhado.
  * Substitui `IPropostaGateway` por **Fake** controlável.
  * `Migrate()` no boot da fixture (e `EnsureDeleted()` antes) para isolamento.

## Observabilidade & Swagger

* Swagger (`/swagger`) ativo em `Development`/`Testing`.
* `MapGet("/")` redireciona para Swagger (DX).
* **Compose**: healthcheck pode apontar para `/swagger/index.html`.
* Logs ASP.NET Core padrão; adicionar Serilog e correlação é próximo passo.

## Segurança (placeholder)

* Este blueprint assume **ambiente dev**.
* Para produção: autenticação/autorizações (JWT/OAuth2), `UseHttpsRedirection`, headers de segurança, rate limiting, validação robusta de input, etc.

## Roadmap / Próximos Passos

* ✅ Paridade de Dockerfile nos dois serviços.
* ✅ `docker-compose.yml` na raiz com wiring e volumes.
* ⏩ `GET /propostas/{id}` no PropostaService e ajuste no `HttpPropostaGateway`.
* ⏩ DTOs de resposta tipados + `.Produces<T>()` para schemas ricos no Swagger.
* ⏩ Observabilidade: Serilog + OpenTelemetry (traces/spans) + métricas.
* ⏩ Event-driven (mensageria) como bônus de arquitetura.

## ADRs

* **ADR-001**: Hexagonal + Minimal APIs como adapters de entrada.
* **ADR-002**: SQLite em dev; conversão de `DateTimeOffset` → UnixTime no ContratacaoService.
* **ADR-003**: Testes de integração com migrations (idempotente) e dublês de gateway.

---
