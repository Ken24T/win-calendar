# Phase 5 Release Readiness Checklist

Last updated: 2026-02-14

## Scope

Checklist for preparing WinCalendar `v0.4.0` after Phase 5 validation and regression uplift.

## Current Version Baseline

- Source of truth: `Directory.Build.props`
- Current version: `0.3.0`
- Next planned release target: `0.4.0`

## Pre-release Gates

- [x] Build succeeds locally (`dotnet build WinCalendar.sln`)
- [x] Full test suite passes (`dotnet test WinCalendar.sln`)
- [x] Problems panel is clean
- [x] `docs/PHASE5_MANUAL_SMOKE_CHECKLIST.md` completed
- [x] All active Phase 5 deltas moved to `Resolved` or `Accepted`

## Version Alignment

- [ ] Update `Directory.Build.props`:
  - `Version`
  - `AssemblyVersion`
  - `FileVersion`
  - `InformationalVersion`
- [ ] Confirm release tag value matches `Version` exactly

## Tag and Workflow Compatibility

Release workflow accepts tags:

- `vX.Y.Z`
- `X.Y.Z`

- [ ] Choose tag format (`0.4.0` preferred)
- [ ] Confirm tag points to commit that introduces version bump

## Release Notes

- [x] Draft release notes for `v0.4.0` (`docs/RELEASE_NOTES_DRAFT_v0.4.0.md`)
- [ ] Finalise release notes for `v0.4.0`
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

- [ ] Execute publish sequence on approved ship command

## Post-release Verification

- [ ] CI workflow green on `main`
- [ ] Release workflow artefact uploaded successfully
- [ ] Local branch sync complete
