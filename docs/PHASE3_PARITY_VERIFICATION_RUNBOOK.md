# Phase 3 Parity Verification Runbook

Last updated: 2026-02-14

## Purpose

Provide a repeatable manual verification flow for the remaining Rust parity checks identified in `docs/PHASE2_MANUAL_PARITY_CHECKLIST.md`.

## Preconditions

- WinCalendar branch: `feature/phase3-parity-hardening`
- Rust reference app available for side-by-side comparison
- Same sample data set loaded in both apps
- Local timezone and system clock aligned before test run
- In WinCalendar, click `Load P3 Data` before executing cases (idempotent seed/update)

## Test Case 1 — Countdown wording thresholds

### Goal

Verify countdown wording parity for overdue and near-due transitions.

### Setup

Create these countdown cards in both apps (WinCalendar can be auto-seeded via `Load P3 Data`):

1. Target `now + 50 hours`
2. Target `now + 26 hours`
3. Target `now + 45 minutes`
4. Target `now - 20 minutes`

### Steps

1. Open countdown panel and record label/status text for each card.
2. Advance waiting window until at least one card crosses each threshold (or adjust target times).
3. Re-check wording immediately after threshold transition.

### Expected

- Status buckets are consistent at transition boundaries.
- Wording remains semantically equivalent to Rust for:
  - overdue card(s)
  - near-due card(s)
  - standard upcoming card(s)

### Result

- [ ] Pass
- [ ] Fail
- Notes:

## Test Case 2 — Countdown ordering ties

### Goal

Verify ordering parity when cards have same sort order and close target times.

### Setup

Create at least 5 cards (WinCalendar can be auto-seeded via `Load P3 Data`):

- 3 cards with same `SortOrder`
- overlapping target times (same day, within 30 minutes)
- mixed active/inactive states

### Steps

1. Enable `Show inactive` in countdown manager.
2. Confirm manager grid ordering.
3. Return to shell countdown panel and confirm active-card ordering.
4. Edit one card target time and one card sort order; save.
5. Confirm re-ordering behaviour in both manager and shell panel.

### Expected

- Manager shows deterministic ordering across ties.
- Shell panel prioritises urgency, then sort order, then target/title deterministically.
- Behaviour is stable across refresh/reopen.

### Result

- [ ] Pass
- [ ] Fail
- Notes:

## Test Case 3 — Dense multi-day PDF formatting

### Goal

Verify PDF readability and grouping parity under high event density.

### Setup

Create a week with (WinCalendar can be auto-seeded via `Load P3 Data`):

- at least 20 events
- mixed all-day and timed events
- multiple categories
- overlapping times on at least 2 days

### Steps

1. Export PDF from Week view.
2. Export PDF from Work Week view.
3. Compare layout against Rust export for grouping/readability.

### Expected

- Day grouping is preserved and readable.
- Columns remain aligned under dense rows.
- No clipped headers or unreadable row content in normal zoom.

### Result

- [ ] Pass
- [ ] Fail
- Notes:

## Test Case 4 — Long title/notes wrapping in PDF

### Goal

Verify parity for long text wrapping/truncation behaviour.

### Setup

Create events with (WinCalendar can be auto-seeded via `Load P3 Data`):

- title length > 100 chars
- multiline notes > 300 chars
- location + notes together

### Steps

1. Export PDF from Day and Month views.
2. Review `Details` column content for truncation/wrapping consistency.
3. Compare against Rust output for equivalent entries.

### Expected

- Output stays readable and does not break table structure.
- Truncation/wrapping is predictable and parity-acceptable.
- Long text does not push other columns out of usable layout.

### Result

- [ ] Pass
- [ ] Fail
- Notes:

## Delta Logging Template

Use this format for any mismatch found:

- Area:
- Scenario:
- Expected (Rust):
- Actual (WinCalendar):
- Severity: High / Medium / Low
- Proposed fix:
- Follow-up test added: Yes / No

## Completion Criteria

Phase 3 parity verification can be marked complete when:

- All 4 test cases pass, or
- Any failures are logged, fixed, and re-verified as pass
- Relevant regression tests are added for fixed defects
