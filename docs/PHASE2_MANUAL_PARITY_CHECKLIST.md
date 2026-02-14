# Phase 2 Manual Parity Checklist

Last updated: 2026-02-14

## Completed in current branch

- [x] Countdown cards persistence, repository, and service flow
- [x] Countdown manager dialog CRUD wiring
- [x] Countdown manager validation feedback (title, colour, sort order, target date)
- [x] Countdown manager inactive-card handling with optional visibility filter
- [x] Countdown card live refresh on a timer in shell
- [x] Countdown urgency states and ordering (Overdue, Due soon, Upcoming)
- [x] PDF export command from shell toolbar
- [x] View-aware PDF metadata (view and range)
- [x] Day-grouped PDF layout
- [x] PDF details column with location and notes preview
- [x] PDF empty-range rendering

## Still to manually verify against Rust app

- [ ] Exact countdown wording parity for overdue and near-due thresholds
- [ ] Countdown ordering parity when multiple cards share sort and close target times
- [ ] PDF formatting parity for dense multi-day schedules
- [ ] PDF text wrapping parity for long titles/notes and mixed category sets

## Notes

- Automated verification currently passes: build succeeds, tests pass, and startup smoke-check passes.
- See also `WinCalendar.Tests/CountdownServiceTests.cs` and `WinCalendar.Tests/PdfExportServiceTests.cs` for current Phase 2 coverage.
