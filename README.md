# WinCalendar

WinCalendar is a C#/.NET 8 + WPF rewrite of the existing Rust desktop calendar at `D:/Development/Repos/rust-calendar`.

## Current Status

- Initial solution scaffolding complete
- Layered architecture in place (`App`, `Domain`, `Application`, `Infrastructure`, `Import`, `Tests`)
- WPF shell with MVVM + DI startup
- Foundation governance files in `.github` including TCTBP workflow

## Build and Run

```powershell
dotnet restore
dotnet build WinCalendar.sln
dotnet test WinCalendar.sln
dotnet run --project WinCalendar.App/WinCalendar.App.csproj
```

## Planned Phases

- **Phase 1**: Core parity (Day/Week/Work Week/Month views, events, recurrence/search, templates, categories, settings/theme basics, ICS import/export, backup/restore)
- **Phase 2**: Countdown cards and PDF export parity
