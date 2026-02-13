# WinCalendar – Copilot Instructions

## Project Overview

Windows desktop calendar rewrite using **C# + .NET 8 + WPF**.

This repo duplicates functional behaviour from `D:/Development/Repos/rust-calendar` in a more traditional Windows stack.

**Delivery approach:**
- **Phase 1:** Core parity (Day/Week/Work Week/Month views, events, recurrence/search, templates, categories, settings/theme basics, ICS import/export, backup/restore)
- **Phase 2:** Countdown cards and PDF export parity

## Repository Structure

| Folder | Purpose |
|--------|---------|
| `/WinCalendar.App` | WPF app shell, XAML, view models |
| `/WinCalendar.Domain` | Core entities and enums |
| `/WinCalendar.Application` | Use cases, service contracts, orchestration |
| `/WinCalendar.Infrastructure` | Persistence and external adapters |
| `/WinCalendar.Import` | Import from Rust app data model into new schema |
| `/WinCalendar.Tests` | Unit/integration tests |

## Development Commands

```powershell
# From repo root
dotnet restore
dotnet build WinCalendar.sln
dotnet test WinCalendar.sln

# Run desktop app
dotnet run --project WinCalendar.App/WinCalendar.App.csproj
```

## Critical Rules

1. **No secrets in source** – never commit credentials, tokens, or machine-specific secrets.
2. **No hard-coded machine paths** in runtime code.
3. **Version sync** – keep version values aligned with release/tag workflows.
4. **Preserve behaviour parity** with the Rust app unless a change is explicitly requested.
5. **Use Australian English** spelling and grammar in user-facing text.

## Architecture Guidance

- Keep UI logic in `WinCalendar.App` view models; keep business logic in `WinCalendar.Application`.
- Domain entities stay in `WinCalendar.Domain` with minimal framework coupling.
- Infrastructure implements interfaces from `WinCalendar.Application`.
- `WinCalendar.Import` handles source-to-target schema migration logic; do not leak import concerns into the UI layer.

## Data and Migration Guidance

- Target is a **new schema** with importer support from Rust SQLite.
- Preserve source semantics for:
  - RFC3339 datetime fields
  - recurrence rule behaviour
  - recurrence exceptions represented as JSON lists
- Make importer operations deterministic and auditable with clear logging.

## Testing Guidance

- Use `xUnit` for tests in `WinCalendar.Tests`.
- Prioritise service and recurrence logic tests before broad UI automation.
- For importer changes, include fixture tests covering date/recurrence conversion and record counts.

## Shipping Workflow

For SHIP/TCTBP activation, steps, approvals, and versioning rules, see [TCTBP Agent.md](TCTBP Agent.md) and [TCTBP.json](TCTBP.json).

## Branch Naming

- `feature/<name>` – New features
- `fix/<name>` – Bug fixes
- `docs/<name>` – Documentation updates
- `infrastructure/<name>` – Build/CI/tooling changes

## When Generating Code

- Prefer strict typing and immutable defaults where practical.
- Keep files focused and small by responsibility.
- Handle loading/empty/error states explicitly.
- Log operational failures with actionable context.
- Avoid broad refactors unless requested.
