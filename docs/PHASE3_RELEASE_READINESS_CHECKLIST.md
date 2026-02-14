# Phase 3 Release Readiness Checklist

Last updated: 2026-02-14

## Scope

Checklist for preparing the next WinCalendar release after Phase 3 parity hardening.

## Current Version Baseline

- Source of truth: `Directory.Build.props`
- Current version: `0.3.0`
- Next planned release target: `0.4.0`

## Pre-release Gates

- [x] Build succeeds locally (`dotnet build WinCalendar.sln`)
- [x] Full test suite passes (`dotnet test WinCalendar.sln`)
- [x] Problems panel is clean
- [x] Remaining Phase 3 parity checks completed in `docs/PHASE2_MANUAL_PARITY_CHECKLIST.md`
- [x] All active deltas in `docs/PHASE3_PARITY_DELTAS.md` moved to `Resolved` or `Accepted`

## Version Alignment

- [x] Update `Directory.Build.props`:
  - `Version`
  - `AssemblyVersion`
  - `FileVersion`
  - `InformationalVersion`
- [x] Confirm release tag value matches `Version` exactly

## Tag and Workflow Compatibility

Release workflow accepts tags:

- `vX.Y.Z`
- `X.Y.Z`

Verification in `.github/workflows/release.yml` strips optional leading `v` and compares against `Directory.Build.props`.

- [x] Choose tag format (`0.3.0`)
- [x] Confirm tag points to commit that introduces version bump

## Release Notes

- [x] Finalise draft notes in `docs/RELEASE_NOTES_DRAFT_v0.3.0.md`
- [x] Confirm user-facing wording uses Australian English
- [x] Confirm notable fixes/features map to shipped commits

## Publish Sequence (TCTBP-aligned)

1. Preflight
2. Test
3. Problems check
4. Bump version
5. Commit
6. Tag
7. Push

- [x] Execute publish sequence on approved ship command

## Post-release Verification

- [x] CI workflow green on `main`
- [x] Release workflow artifact uploaded successfully (`wincalendar-0.3.0`)
- [x] Local branch sync complete
