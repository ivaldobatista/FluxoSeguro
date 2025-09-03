# FluxoSeguro — Proposta & Contratação (Hexagonal, .NET 9)

> **North Star:** blueprint de plataforma de seguros com **Arquitetura Hexagonal (Ports & Adapters)**, **DDD**, **Clean Architecture**, **.NET 9**, **EF Core + SQLite**, **Swagger/OpenAPI** e **testes (unit e integração)**. Dois microserviços:
>
> - **PropostaService** — cria/lista/atualiza status de propostas.
> - **ContratacaoService** — efetiva contratação **apenas** se a proposta estiver **Aprovada**, consultando o PropostaService via HTTP.

## Sumário

- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Estrutura de Pastas](#estrutura-de-pastas)
- [Rodando em Dev (sem Docker)](#rodando-em-dev-sem-docker)
- [Subindo com Docker Compose](#subindo-com-docker-compose)
- [APIs — Quick Reference](#apis--quick-reference)
- [Smoke Test E2E (curl)](#smoke-test-e2e-curl)
- [Testes Automatizados](#testes-automatizados)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Troubleshooting](#troubleshooting)
- [Documentação adicional](#documentação-adicional)

## Arquitetura

```mermaid

flowchart LR

    subgraph PropostaService [PropostaService]
      A1[API (Minimal APIs)]
      A2[Application\n(IPropostaUseCases)]
      A3[Domain\n(Proposta, Regras)]
      A4[Infrastructure\n(EF Core Repo)]
      A1 --> A2 --> A3
      A2 --> A4 --> DB1[(SQLite)]
    end

    subgraph ContratacaoService [ContratacaoService]
      B1[API (Minimal APIs)]
      B2[Application\n(IContratacaoUseCases)]
      B3[Domain\n(Contratacao)]
      B4[Infrastructure\n(EF Repo)]
      B5[Infrastructure\n(HttpPropostaGateway)]
      B1 --> B2 --> B3
      B2 --> B4 --> DB2[(SQLite)]
    end

    B5 <-- HTTP REST --> A1
    B2 --> B5

```

## Pré-requisitos

* .NET SDK **9.0+**
* Docker Desktop (se for usar **compose**)
* PowerShell/Bash para scripts/curl

## Estrutura de Pastas

```
/                  # raiz do monorepo
├─ docker-compose.yml
├─ README.md
├─ doc/            # documentação detalhada (design/ADRs/how-to)
├─ src/
│  ├─ PropostaService/
│  │  ├─ Domain/ Application/ Infrastructure/ ...
│  │  ├─ data/                # SQLite (dev)
│  │  └─ Dockerfile
│  └─ ContratacaoService/
│     ├─ Domain/ Application/ Infrastructure/ ...
│     ├─ data/
│     └─ Dockerfile
└─ tests/
   ├─ PropostaService.Tests/
   └─ ContratacaoService.Tests/
```

## Rodando em Dev (sem Docker)

Em **dois terminais**:

### 1) PropostaService

```bash
cd src/PropostaService
dotnet ef database update         # garante schema (migrations)
dotnet run --launch-profile http  # sobe em http://localhost:5024
```

Swagger: [http://localhost:5024/swagger](http://localhost:5024/swagger)

### 2) ContratacaoService

```bash
cd src/ContratacaoService
# Se o PropostaService estiver em outra porta/host, informe a base:
# setx PropostaService__BaseUrl "http://localhost:5024"  (Windows, novo terminal)
# export PropostaService__BaseUrl="http://localhost:5024" (macOS/Linux)

dotnet ef database update
dotnet run --launch-profile http  # sobe em http://localhost:5034 (ajuste conforme seu profile)
```

Swagger: [http://localhost:5034/swagger](http://localhost:5034/swagger)

> Ambos os serviços têm uma rota de conveniência `/` → redireciona para `/swagger`.

## Subindo com Docker Compose

Na **raiz** do repositório:

```bash
docker compose up --build -d
```

Endpoints:

* PropostaService → [http://localhost:5024/swagger](http://localhost:5024/swagger)
* ContratacaoService → [http://localhost:5034/swagger](http://localhost:5034/swagger)

O `docker-compose.yml` mapeia volumes `*_data` para persistir os arquivos `.db` em `/app/data`.

## APIs — Quick Reference

### PropostaService

* **POST** `/propostas`
  Body:

  ```json
  { "nomeCliente": "Alice", "valor": 1200.0 }
  ```

  201 → `{ "id": "<guid>" }`

* **GET** `/propostas`
  200 → `{ "items": [ ... ], "count": 1 }`

* **PUT** `/propostas/{id}/status`
  Body (enum padrão **numérico**):

  ```json
  { "status": 1 }   // 0=EmAnalise, 1=Aprovada, 2=Rejeitada
  ```

  204 (ok) | 404 (não encontrada) | 400 (payload inválido)

### ContratacaoService

* **POST** `/contratacoes`
  Body:

  ```json
  { "propostaId": "<guid>" }
  ```

  201 → `{ "id": "<guid>" }`
  400 (não aprovada) | 404 (proposta inexistente)

* **GET** `/contratacoes`
  200 → `{ "items": [ ... ], "count": N }`

> Obs: No ContratacaoService, o campo `DataContratacao` é mapeado como **UnixTime (INTEGER)** no SQLite para suportar `ORDER BY` com EF Core.

## Smoke Test E2E (curl)

```bash
# 1) Criar proposta
curl -s -X POST http://localhost:5024/propostas \
  -H "Content-Type: application/json" \
  -d '{ "nomeCliente": "Alice", "valor": 1500 }'

# capture o ID (ex.: PROPOSTA_ID)

# 2) Aprovar
curl -i -X PUT http://localhost:5024/propostas/PROPOSTA_ID/status \
  -H "Content-Type: application/json" \
  -d '{ "status": 1 }'   # 1 = Aprovada

# 3) Contratar
curl -s -X POST http://localhost:5034/contratacoes \
  -H "Content-Type: application/json" \
  -d "{ \"propostaId\": \"PROPOSTA_ID\" }"
```

## Testes Automatizados

```bash
dotnet test
```

Cobertura via `coverlet.collector` (habilitado nos projetos de teste). Os testes de integração usam `WebApplicationFactory`, SQLite in-memory e um **FakePropostaGateway** no ContratacaoService.

## Variáveis de Ambiente

| Serviço            | Variável                   | Default                 | Uso                                  |
| ------------------ | -------------------------- | ----------------------- | ------------------------------------ |
| ContratacaoService | `PropostaService__BaseUrl` | `http://localhost:5024` | BaseAddress do HttpClient do gateway |
| Ambos              | `ASPNETCORE_ENVIRONMENT`   | `Development`           | Habilita Swagger UI, etc.            |
| Ambos (container)  | `ASPNETCORE_URLS`          | `http://+:8080`         | Bind Kestrel                         |

## Troubleshooting

* **Swagger não abre no VS:** selecione o profile **http/https** do **Project** (não o Docker), e verifique `launchSettings.json` (`launchUrl: "swagger"`).
* **Perfil `Container (Dockerfile)` abre porta aleatória:** esperado. O VS publica `8080`/`8081` do container em portas randômicas (ex.: `32786/32787`). Use **Docker Compose** para portas fixas.
* **SQLite & ORDER BY em `DateTimeOffset`:** resolvido com `HasConversion` para UnixTime no ContratacaoService.
* **Conflito `EnsureCreated` vs `Migrate` nos testes:** os factories de teste usam **migrations** (idempotente); não misturar com `EnsureCreated` no mesmo DB.

## Documentação adicional

Veja **[`/doc/README.md`](doc/README.md)** para detalhamento de arquitetura, camadas, decisões e extensões futuras.

---
