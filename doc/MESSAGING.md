# Mensageria — RabbitMQ

Esta visão detalha a *port* de publicação de eventos do **PropostaService** e como instrumentar o broker para *observability* local.

## Exchange & Routing Keys

- **Exchange**: `propostas.events` (tipo **topic**, durável)
- **Producer**: PropostaService
- **Routing keys**:
  - `proposta.created` — emitido após criação da proposta (201)

### Schema (exemplo)

```json
{
  "propostaId": "e1f6a37a-8a78-4d53-bb8a-5d886ab7c7c0",
  "nomeProponente": "Alice",
  "valor": 1500.0,
  "dataCriacao": "2025-09-04T12:34:56Z"
}
````

## Configuração (env)

```ini
RabbitMQ__HostName=rabbitmq
RabbitMQ__Port=5672
RabbitMQ__UserName=guest
RabbitMQ__Password=guest
RabbitMQ__VirtualHost=/
RabbitMQ__Exchange=propostas.events
RabbitMQ__ExchangeType=topic
```

> **Pacote**: `RabbitMQ.Client` **6.6.0** (API síncrona com `IConnection`/`IModel`).
> A conexão é criada no *publisher* e o *publish* é **fire-and-forget** (logs de warning em falha).


---
