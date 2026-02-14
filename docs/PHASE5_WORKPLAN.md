# Phase 5 Workplan — v0.4.0 Planning and Delivery

Last updated: 2026-02-14

## Goal

Define and deliver the next stable release (`v0.4.0`) after Phase 4 closeout, with emphasis on workflow reliability and release confidence.

## Scope

1. Scope lock for Phase 5 deliverables
2. End-to-end workflow validation and targeted fixes
3. Regression coverage uplift for user-critical paths
4. Release execution and verification (`v0.4.0`)

## Phase 5 Backlog

## 1) Scope and Acceptance

- [x] Confirm Phase 5 deliverables and non-goals
- [x] Define release acceptance criteria for `v0.4.0`
- [x] Link implementation tasks to verification evidence

## 2) Workflow Validation

- [x] Execute `docs/PHASE5_MANUAL_SMOKE_CHECKLIST.md`
- [x] Run manual smoke pass for Day/Week/Work Week/Month navigation
- [x] Verify core dialogs and commands (new/edit/search/countdown/categories/templates/export)
- [x] Record any issues as deltas and resolve High severity items (`docs/PHASE5_DELTAS.md`)

## 3) Regression Uplift

- [x] Add one focused recurrence/edit-flow regression test
- [x] Add one focused import/settings persistence regression test
- [x] Keep full suite green after each targeted addition

## 4) Release Execution (`v0.4.0`)

- [x] Prepare release readiness checklist (`docs/PHASE5_RELEASE_READINESS_CHECKLIST.md`)
- [ ] Execute release readiness checklist
- [ ] Bump version metadata and tag per SHIP/TCTBP flow
- [ ] Validate CI and release artefact output

## Exit Criteria

- Phase 5 backlog complete or explicitly deferred with rationale
- `v0.4.0` shipped and validated
- No open High severity issues at phase closeout
