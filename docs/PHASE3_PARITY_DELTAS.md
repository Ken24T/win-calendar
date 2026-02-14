# Phase 3 Parity Delta Tracker

Last updated: 2026-02-14

## Purpose

Capture WinCalendar vs Rust parity mismatches discovered during Phase 3 verification and track their closure.

## Status Key

- `Open` — identified and not yet fixed
- `In Progress` — fix underway
- `Resolved` — fix merged and re-verified
- `Accepted` — intentional behaviour difference agreed

## Delta Register

| ID | Area | Scenario | Expected (Rust) | Actual (WinCalendar) | Severity | Status | Owner | Fix PR/Commit | Notes |
|---|---|---|---|---|---|---|---|---|---|
| P3-D01 | Countdown wording | Overdue threshold text | _TBD during manual pass_ | _TBD during manual pass_ | Medium | Open | - | - | Seed via `Load P3 Data` |
| P3-D02 | Countdown ordering | Tie ordering with same sort/time | _TBD during manual pass_ | _TBD during manual pass_ | Medium | Open | - | - | Validate manager + shell panel |
| P3-D03 | PDF dense layout | High-volume week export readability | _TBD during manual pass_ | _TBD during manual pass_ | Medium | Open | - | - | Compare week + work week exports |
| P3-D04 | PDF wrapping | Long title/notes wrapping | _TBD during manual pass_ | _TBD during manual pass_ | Medium | Open | - | - | Validate day + month exports |
| P3-D05 | P3 data seeding | Load P3 Data action should complete without runtime errors | Seed operation succeeds | Runtime error: invalid GUID format from dense-event seed ID | High | Resolved | Copilot | _pending commit_ | Fixed GUID format in dense-event seed IDs |

## Re-Verification Log

| Delta ID | Re-test Date | Result | Verified By | Evidence |
|---|---|---|---|---|
| - | - | - | - | - |

## Exit Criteria

Phase 3 parity can be closed when all `Open` and `In Progress` deltas are either:

- moved to `Resolved` with re-verification evidence, or
- moved to `Accepted` with explicit rationale and approval note.
