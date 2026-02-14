# WinCalendar v0.4.0 (Draft)

Last updated: 2026-02-14

## Summary

This release focuses on Phase 5 validation depth and release confidence after the `v0.3.0` quality uplift.

## Highlights

### Workflow validation

- Added a dedicated Phase 5 manual smoke checklist covering views, navigation, dialogs, recurrence, and diagnostics.
- Completed a full manual smoke pass for core flows with outcome tracking and sign-off notes.
- Captured and triaged keyboard-shortcut visibility observation as an accepted low-severity delta.

### Regression uplift

- Added recurrence edit-flow persistence regression for updates to existing events.
- Added importer/settings re-import persistence regression to prevent duplicate setting keys and verify updated values.
- Kept full build and test suite green after each targeted addition.

### Release readiness

- Added a dedicated `v0.4.0` release-readiness checklist.
- Completed pre-release technical gates for build, tests, and problems cleanliness.

## Upgrade Notes

- No schema-breaking migration changes introduced in this phase.
- Existing data remains compatible with current migrations and importer behaviour.

## Known Follow-up Items

- Execute SHIP/TCTBP version bump, tag, and push flow for `v0.4.0`.
- Verify release workflow artefact upload on tagged release run.

## Build and Test Baseline

- Solution builds successfully on .NET 8.
- Test suite currently passing in local verification.
