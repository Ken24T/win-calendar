# Phase 4 Workplan — Release and Quality

Last updated: 2026-02-14

## Goal

Ship the next stable WinCalendar release and strengthen quality after parity completion.

## Scope

1. Release execution (`v0.3.0` target)
2. Post-release stability and diagnostics review
3. Focused UX polish for readability and workflow speed
4. Performance and regression hardening

## Phase 4 Backlog

## 1) Release Execution

- [x] Execute `docs/PHASE3_RELEASE_READINESS_CHECKLIST.md`
- [x] Finalise `docs/RELEASE_NOTES_DRAFT_v0.3.0.md`
- [x] Perform SHIP/TCTBP bump/tag/push flow
- [x] Verify release workflow artifact output

## 2) Stability and Operations

- [x] Review startup diagnostics logs under real usage
- [x] Tune any noisy non-critical error paths
- [x] Add one operational troubleshooting note for common startup/runtime failures

## 3) UX Polish (Targeted)

- [x] Improve dense Month-cell readability under heavy event load
- [x] Verify countdown panel readability for long titles and narrow window widths
- [x] Confirm keyboard shortcuts + dialog flows remain consistent after recent additions

## 4) Performance + Regression

- [x] Add one focused regression test around high-density calendar rendering setup data
- [x] Add one focused regression test around countdown refresh/order stability over repeated refreshes
- [x] Run full build/test/problematics pass before Phase 4 closeout

## Exit Criteria

- Release `v0.3.0` published and validated
- No open High severity issues from Phase 4 work
- All checklist items above complete or explicitly deferred with rationale
