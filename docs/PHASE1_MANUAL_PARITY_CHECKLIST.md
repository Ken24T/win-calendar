# Phase 1 Manual Parity Checklist

Last updated: 2026-02-14

## Completed in current branch

- [x] Day, Week, Work Week, Month view switching and range navigation
- [x] Create event from toolbar
- [x] Edit event from Day/Week/Work Week/Month views
- [x] Delete event from event editor
- [x] Search dialog with direct edit action
- [x] Category and template management dialogs
- [x] Recurrence rule text editing
- [x] Recurrence exceptions editing and persistence
- [x] Explicit start/end time editing with validation
- [x] All-day mode disables time fields and saves full-day range
- [x] Month date click creates event seeded to selected day
- [x] Month view highlights today
- [x] Keyboard shortcuts for navigation, view switching, create/search, and refresh

## Still to manually verify against Rust app

- [x] Exact date/time formatting parity across all views
- [x] All-day multi-day event rendering behaviour in month and week views
- [x] Recurrence edge cases (BYDAY/BYMONTHDAY combinations) in UI workflows
- [x] Empty-state messaging and loading/error state wording parity

## Notes

- Build and tests pass after each completed item in this document.
- Problems panel currently has no active diagnostics.
