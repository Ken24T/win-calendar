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
| P3-D01 | Countdown wording | Overdue threshold text | Threshold wording should be parity-acceptable | User manual check looked correct for seeded overdue/near-due states | Medium | Resolved | User + Copilot | `5d2aeeb` | Seed via `Load P3 Data` |
| P3-D02 | Countdown ordering | Tie ordering with same sort/time | Ordering should remain stable and deterministic | User manual check looked stable in manager + shell panel | Medium | Resolved | User + Copilot | `2e96978` | Validate manager + shell panel |
| P3-D03 | PDF dense layout | High-volume week export readability | Readable grouped output under dense schedules | User manual spot-check reported output looks ok | Medium | Resolved | User + Copilot | `ebc3c08` | Validate week/work-week exports in broader pass if needed |
| P3-D04 | PDF wrapping | Long title/notes wrapping | Long text should remain readable without broken layout | User manual spot-check reported output looks ok | Medium | Resolved | User + Copilot | `ebc3c08` | Validate day/month exports in broader pass if needed |
| P3-D05 | P3 data seeding | Load P3 Data action should complete without runtime errors | Seed operation succeeds | Runtime error: invalid GUID format from dense-event seed ID | High | Resolved | Copilot | `810f210` | Re-verified successfully after fix |

## Re-Verification Log

| Delta ID | Re-test Date | Result | Verified By | Evidence |
|---|---|---|---|---|
| P3-D05 | 2026-02-14 | Pass | User + Copilot | Manual rerun of `Load P3 Data` after fix; app remains stable |
| P3-D03 | 2026-02-14 | Pass | User | Manual PDF export spot-check looked correct |
| P3-D04 | 2026-02-14 | Pass | User | Manual PDF export spot-check looked correct |
| P3-D01 | 2026-02-14 | Pass | User | Manual countdown wording check looked correct |
| P3-D02 | 2026-02-14 | Pass | User | Manual countdown ordering check looked correct |

## Exit Criteria

Phase 3 parity can be closed when all `Open` and `In Progress` deltas are either:

- moved to `Resolved` with re-verification evidence, or
- moved to `Accepted` with explicit rationale and approval note.
