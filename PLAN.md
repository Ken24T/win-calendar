# WinCalendar Implementation & Migration Plan

Last updated: 2026-02-14

## Objective

Replicate functional behaviour from `D:/Development/Repos/rust-calendar` in a Windows-traditional stack using C#/.NET 8 + WPF.

## Phase Overview

### Phase 0 — Governance and delivery workflow

- [x] Port and adapt `.github/copilot-instructions.md`
- [x] Add `TCTBP Agent.md` and `TCTBP.json`
- [x] Adapt CI/release workflows for .NET solution
- [x] Align release tag trigger with TCTBP tag format
- [x] Add `.gitignore` and remove tracked build artefacts

### Phase 1 — Core parity (current target)

#### 1. Foundation (completed)
- [x] Create solution and layered projects (`App`, `Domain`, `Application`, `Infrastructure`, `Import`, `Tests`)
- [x] Wire DI/host startup in WPF app
- [x] Add initial shell VM and event service scaffolding

#### 2. Persistence and schema (in progress)
- [x] Define target SQLite schema for core entities (events, categories, templates, settings, themes)
- [x] Add migration strategy for schema evolution
- [x] Replace in-memory repositories with SQLite-backed repositories

#### 3. Rust database import (completed)
- [x] Create deterministic importer from Rust SQLite to new schema
- [x] Preserve RFC3339 datetime semantics
- [x] Preserve recurrence exception semantics (JSON lists)
- [x] Add importer validation report (counts + conversion summary)

#### 4. Core service parity
- [ ] Event CRUD and range queries
- [x] Recurrence expansion parity
- [x] Search parity
- [x] Categories/templates/settings basics service layer
- [ ] Theme service parity
- [x] ICS import/export parity
- [x] Backup/restore parity

#### 5. UI parity for core views
- [ ] Day/Week/Work Week/Month navigation and rendering
- [ ] Event create/edit dialogs
- [ ] Search and management dialogs

#### 6. Verification for phase 1
- [x] Unit/integration tests for services and recurrence
- [ ] Import fixtures and conversion tests
- [ ] Manual parity checklist against Rust app

### Phase 2 — Extended parity

- [ ] Countdown cards subsystem parity
- [ ] PDF export parity
- [ ] Additional test coverage and performance checks

## Branching and workflow

- Current implementation branch: `feature/phase1-persistence-import-foundation`
- Mainline branch: `main`
- Branch naming:
  - `feature/<name>` for feature work
  - `fix/<name>` for bug fixes
  - `docs/<name>` for documentation
  - `infrastructure/<name>` for tooling/CI

## Progress tracking rules

- Keep this file updated when phase status changes.
- Update task checkboxes only when verified by build/test or explicit functional validation.
- Use SHIP/Handoff workflows for milestone transitions and synchronisation.
