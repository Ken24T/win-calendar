# Phase 5 Manual Smoke Checklist

Last updated: 2026-02-14

## Purpose

Manual smoke validation for core user workflows before shipping `v0.4.0`.

## Calendar Views and Navigation

- [x] Day view loads and shows expected events
- [x] Week view loads and renders correct 7-day range
- [x] Work Week view loads and renders correct 5-day range
- [x] Month view loads and dense cells remain readable (`+N more` indicator)
- [x] Previous/Today/Next navigation updates range correctly
- [ ] Keyboard shortcuts work (`Ctrl+1..4`, `Ctrl+Left/Right`, `Ctrl+T`, `F5`) — `Ctrl+T` and `F5` appeared to have no visible effect in current manual run

## Dialog and Command Flows

- [x] New event dialog opens (`Ctrl+N`) and saves correctly
- [x] Edit event dialog opens from Day/Week/Month entries and saves correctly
- [x] Search dialog opens (`Ctrl+F`) and supports edit-from-result flow
- [x] Countdown manager opens and saves card updates
- [x] Category manager opens and saves category updates
- [x] Template manager opens and saves template updates
- [x] Export PDF command produces valid output file for current view

## Recurrence and Data Integrity

- [x] Recurrence rule changes persist after editing existing event
- [x] Recurrence exceptions persist and are respected in rendered occurrences
- [x] `Load P3 Data` completes without runtime errors

## Diagnostics and Stability

- [x] App starts without fatal error dialog
- [x] App exits cleanly without user-visible exception dialog
- [x] Startup diagnostics log contains no unexpected `ERROR` entries for current run

## Sign-off

- Verified by: User + Copilot
- Verification date: 2026-02-14
- Notes: Core smoke pass successful. Follow-up retained for keyboard shortcut visibility (`Ctrl+T`, `F5`).
