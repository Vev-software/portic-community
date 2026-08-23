# Atlas Community Integration Guide

This guide documents how Atlas Community Edition integrates with Portic as an AI provider.

## Overview

Atlas Community uses Portic as an AI provider through the `IAiProviderExtension` interface. The adapter calls `POST /v1/messages` with the Portic `ChatRequest` contract.

## Gateway Contract

### Endpoint

```
POST /v1/messages
Content-Type: application/json
```

### Request — `ChatRequest`

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `model` | `string` | Yes | Logical model identifier (e.g. `stub-echo`) |
| `messages` | `ChatMessage[]` | Yes | Ordered conversation history: `[{ role: string, content: string }]` |
| `maxTokens` | `int?` | No | Soft cap on generated tokens. Adapters honor best-effort. |
| `provider` | `string?` | No | Explicit provider name. Defaults to configured default when omitted. |

Source: [`Portic.Sdk` NuGet package](https://www.nuget.org/packages/Portic.Sdk) · [`ChatRequest.cs`](https://github.com/Vev-software/portic-sdk/blob/main/contracts/dotnet/Portic.Sdk/Contracts/ChatRequest.cs) · [`ChatMessage.cs`](https://github.com/Vev-software/portic-sdk/blob/main/contracts/dotnet/Portic.Sdk/Contracts/ChatMessage.cs)

### Response — `ChatCompletion`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string` | Gateway-assigned completion id |
| `model` | `string` | Logical model that produced the completion |
| `provider` | `string` | Name of the provider adapter that served the request |
| `message` | `ChatMessage` | The assistant message (`{ role, content }`) |
| `usage` | `TokenUsage` | Token accounting: `{ inputTokens: int, outputTokens: int }` |

Source: [`Portic.Sdk` NuGet package](https://www.nuget.org/packages/Portic.Sdk) · [`ChatCompletion.cs`](https://github.com/Vev-software/portic-sdk/blob/main/contracts/dotnet/Portic.Sdk/Contracts/ChatCompletion.cs) · [`TokenUsage.cs`](https://github.com/Vev-software/portic-sdk/blob/main/contracts/dotnet/Portic.Sdk/Contracts/TokenUsage.cs)

## Authentication Model

Portic does **not** require per-request API key authentication from callers. Provider credentials are configured inside the gateway via environment variables per adapter. External callers send requests to the gateway without auth headers.

## Configuration for Atlas Integration

When a self-hoster enables Portic in Atlas Community:

| Setting | Value |
| --- | --- |
| `Atlas:Portic:BaseUrl` | URL of the Portic gateway (e.g. `http://portic:8080`) |
| Endpoint suffix | The adapter appends `/v1/messages` to the base URL |
| `ApiKey` | Not required — the gateway handles provider auth internally |

## Example curl Request

This is the shape Atlas sends to Portic:

```bash
curl -X POST http://localhost:8080/v1/messages \
  -H 'Content-Type: application/json' \
  -d '{
    "model": "stub-echo",
    "messages": [
      { "role": "system", "content": "Answer only from the grounded Atlas facts you are given." },
      { "role": "user", "content": "What is in this landscape?" }
    ],
    "maxTokens": 1024
  }'
```

Expected response:

```json
{
  "id": "stub-abc123",
  "model": "stub-echo",
  "provider": "stub",
  "message": {
    "role": "assistant",
    "content": "echo: What is in this landscape?"
  },
  "usage": {
    "inputTokens": 12,
    "outputTokens": 5
  }
}
```

## Source References

- `ChatRequest` — [`Portic.Sdk` contract source](https://github.com/Vev-software/portic-sdk/blob/main/contracts/dotnet/Portic.Sdk/Contracts/ChatRequest.cs)
- `ChatCompletion` — [`Portic.Sdk` contract source](https://github.com/Vev-software/portic-sdk/blob/main/contracts/dotnet/Portic.Sdk/Contracts/ChatCompletion.cs)
- `ChatMessage` — [`Portic.Sdk` contract source](https://github.com/Vev-software/portic-sdk/blob/main/contracts/dotnet/Portic.Sdk/Contracts/ChatMessage.cs)
- `TokenUsage` — [`Portic.Sdk` contract source](https://github.com/Vev-software/portic-sdk/blob/main/contracts/dotnet/Portic.Sdk/Contracts/TokenUsage.cs)
