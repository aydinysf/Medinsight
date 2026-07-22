# MedInsight

**MedInsight is a Clinical Decision Support System (CDSS).**

It organizes, compares and analyzes medical records to help physicians and patients make better-informed decisions.

> ⚠️ **MedInsight is NOT a diagnostic tool.** It never claims to diagnose disease. All output is decision *support* — the clinical judgment and final decision always belong to the physician.

## Architecture

.NET 9 · Clean Architecture · DDD · SOLID · CQRS-ready · PostgreSQL (EF Core) · Docker

```
MedInsight.sln
├── docs/                          # Canonical documentation (ADRs, domain, architecture, AI)
└── src/
    ├── MedInsight.Domain            # Aggregates, domain events, business rules. References nothing.
    ├── MedInsight.Application       # Use cases (commands/queries), abstractions. → Domain
    ├── MedInsight.Infrastructure    # EF Core / PostgreSQL, object storage. → Application, Domain
    ├── MedInsight.AIOrchestration   # Hızır AI orchestration layer (ADR-015). → Domain
    ├── MedInsight.TimelineService   # Passive event subscriber, append-only timeline (ADR-006). → Domain
    ├── MedInsight.Dicom             # DICOM parsing/grouping for ingestion. → Domain
    └── MedInsight.Api               # HTTP host, Swagger, health checks. → Application, Infrastructure
```

Docs-first: code follows the documents in `docs/` — domain rule changes update the relevant `docs/domain/` file in the same PR, architectural decisions get an ADR first (see `docs/adr/`). The roadmap lives at `docs/business/roadmap.md`.

Dependency rule: source code dependencies only point inward. `Domain` has no references; `Application` knows only `Domain`; `Infrastructure` implements `Application` abstractions.

## Getting started

### With Docker

```bash
docker compose up --build
```

- API: http://localhost:8080
- Swagger UI (Development): http://localhost:8080/swagger
- PostgreSQL: localhost:5432 (`medinsight` / `medinsight`)

### Local development

Requires the .NET 9 SDK and a running PostgreSQL (or `docker compose up postgres`).

```bash
dotnet build
dotnet run --project src/MedInsight.Api
```

## Endpoints

| Endpoint        | Purpose                                        |
|-----------------|------------------------------------------------|
| `GET /health`   | Liveness — returns `Healthy` (200 OK)          |
| `GET /health/ready` | Readiness — includes PostgreSQL connectivity |
| `GET /swagger`  | OpenAPI documentation (Development only)       |

## AI service (future)

`MedInsight.Application.Abstractions.Ai.IAiService` defines the contract for a future Python AI analysis service. No implementation exists yet; the client will live in `MedInsight.Infrastructure` and communicate over HTTP/gRPC. AI output is analytical support only and is never presented as a diagnosis.

## Configuration

Connection string key: `ConnectionStrings:MedInsightDb` (env var `ConnectionStrings__MedInsightDb` in containers). Development credentials in `appsettings.json` / `docker-compose.yml` are placeholders — override them outside local development.

## License

[MIT](LICENSE)
