# Phase 5 Manual Smoke Checklist

Last updated: 2026-02-14

## Purpose

Manual smoke validation for core user workflows before shipping `v0.4.0`.

## Calendar Views and Navigation

- [ ] Day view loads and shows expected events
- [ ] Week view loads and renders correct 7-day range
- [ ] Work Week view loads and renders correct 5-day range
- [ ] Month view loads and dense cells remain readable (`+N more` indicator)
- [ ] Previous/Today/Next navigation updates range correctly
- [ ] Keyboard shortcuts work (`Ctrl+1..4`, `Ctrl+Left/Right`, `Ctrl+T`, `F5`)

## Dialog and Command Flows

- [ ] New event dialog opens (`Ctrl+N`) and saves correctly
- [ ] Edit event dialog opens from Day/Week/Month entries and saves correctly
- [ ] Search dialog opens (`Ctrl+F`) and supports edit-from-result flow
- [ ] Countdown manager opens and saves card updates
- [ ] Category manager opens and saves category updates
- [ ] Template manager opens and saves template updates
- [ ] Export PDF command produces valid output file for current view

## Recurrence and Data Integrity

- [ ] Recurrence rule changes persist after editing existing event
- [ ] Recurrence exceptions persist and are respected in rendered occurrences
- [ ] `Load P3 Data` completes without runtime errors

## Diagnostics and Stability

- [ ] App starts without fatal error dialog
- [ ] App exits cleanly without user-visible exception dialog
- [ ] Startup diagnostics log contains no unexpected `ERROR` entries for current run

## Sign-off

- Verified by:
- Verification date:
- Notes:
