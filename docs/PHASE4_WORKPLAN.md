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

- [ ] Execute `docs/PHASE3_RELEASE_READINESS_CHECKLIST.md`
- [ ] Finalise `docs/RELEASE_NOTES_DRAFT_v0.3.0.md`
- [ ] Perform SHIP/TCTBP bump/tag/push flow
- [ ] Verify release workflow artifact output

## 2) Stability and Operations

- [ ] Review startup diagnostics logs under real usage
- [ ] Tune any noisy non-critical error paths
- [ ] Add one operational troubleshooting note for common startup/runtime failures

## 3) UX Polish (Targeted)

- [ ] Improve dense Month-cell readability under heavy event load
- [ ] Verify countdown panel readability for long titles and narrow window widths
- [ ] Confirm keyboard shortcuts + dialog flows remain consistent after recent additions

## 4) Performance + Regression

- [ ] Add one focused regression test around high-density calendar rendering setup data
- [ ] Add one focused regression test around countdown refresh/order stability over repeated refreshes
- [ ] Run full build/test/problematics pass before Phase 4 closeout

## Exit Criteria

- Release `v0.3.0` published and validated
- No open High severity issues from Phase 4 work
- All checklist items above complete or explicitly deferred with rationale
