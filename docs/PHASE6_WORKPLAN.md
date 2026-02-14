# Phase 6 Workplan — UX Foundation and Theming Controls

Last updated: 2026-02-14

## Goal

Establish the first UX/UI modernisation baseline while preserving behaviour parity, starting with menu structure and theme mode control.

## Scope

1. Top-level menu structure (wired + stubs)
2. Theme mode switching (`Light`, `Dark`, `System`)
3. Persisted theme mode and startup apply
4. Day/Month baseline surface tokenisation
5. Sidebar visibility and position baseline (Phase 6.1)
6. Comprehensive themed surface compliance pass across all dialogs and views

## Phase 6 Backlog

## 1) Menu Foundation

- [x] Add top-level menu (`File`, `Edit`, `View`, `Events`, `Help`)
- [x] Wire currently supported actions to existing commands
- [x] Add placeholder stubs for not-yet-implemented menu actions

## 2) Theme Modes

- [x] Implement `View -> Themes -> Light`
- [x] Implement `View -> Themes -> Dark`
- [x] Implement `View -> Themes -> System`
- [x] Persist selected mode in settings
- [x] Apply mode on startup

## 3) Baseline Theme Surface Coverage

- [x] Apply theme resources to shell chrome (menu, title/control bands, state overlays)
- [x] Apply theme resources to Day view baseline surface
- [x] Apply theme resources to Month view baseline surface

## 4) Sidebar Follow-up (Phase 6.1)

- [x] Add `View -> Show Sidebar` toggle
- [x] Implement right-docked sidebar container
- [x] Persist sidebar visibility state

## 5) Comprehensive Theme Compliance

- [x] Remove remaining hardcoded view/dialog colours from calendar and dialog XAML
- [x] Add shared implicit styles for themed controls (input/list/popup/calendar states)
- [x] Ensure runtime theme application updates all new token brushes
- [x] Define visual sign-off gate before custom themes
- [x] Publish checklist: [PHASE6_THEME_VISUAL_SIGNOFF_CHECKLIST.md](PHASE6_THEME_VISUAL_SIGNOFF_CHECKLIST.md)

## Exit Criteria

- Menu foundation in place with no command regressions
- Light/Dark/System toggles function and persist across restart
- Day/Month/Week/Work Week and core dialogs remain readable in light and dark modes
- Sidebar plan and acceptance criteria captured for Phase 6.1
- Theme visual sign-off checklist executed before custom theme implementation
