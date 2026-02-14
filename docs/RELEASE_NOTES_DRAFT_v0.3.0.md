# WinCalendar v0.3.0 (Draft)

Last updated: 2026-02-14

## Summary

This release focuses on Phase 2 parity completion and Phase 3 hardening foundations.

## Highlights

### Countdown parity improvements

- Added countdown card persistence and application-layer services.
- Added countdown manager dialog with CRUD support.
- Added validation feedback for title, colour, sort order, and target date.
- Added inactive-card management with optional visibility filtering.
- Added live countdown refresh and urgency-aware ordering.
- Centralised threshold/status logic with boundary regression tests.

### PDF export parity improvements

- Added PDF export command from the main toolbar.
- Added view-aware export metadata (view and range context).
- Added day-grouped PDF layout for readability.
- Added details column for location and notes preview.
- Added empty-range handling and long-notes export test coverage.

### Reliability and diagnostics

- Added startup diagnostics with safer production defaults.
- Added diagnostics log rollover, archive retention, and age-based pruning.
- Enabled verbose info diagnostics only when debugger attached or `WINCALENDAR_DIAGNOSTICS_VERBOSE=1`.

### Verification and process

- Added Phase 2 manual parity checklist.
- Added Phase 3 verification runbook and parity delta tracker.
- Added deterministic in-app `Load P3 Data` seed workflow for repeatable verification.

## Upgrade Notes

- No migration action required beyond normal startup database migration.
- Existing data remains compatible with current schema migrations.

## Known Follow-up Items

- Complete ship version bump/tag/push sequence for `v0.3.0`.
- Verify release workflow artifact upload on tagged release run.

## Build and Test Baseline

- Solution builds successfully on .NET 8.
- Test suite currently passing in CI/local validation.
