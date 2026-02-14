# Phase 6 Theme Visual Sign-off Checklist

Last updated: 2026-02-14

## Purpose

Provide a single, repeatable sign-off run for theme compliance before any custom theme work begins.

## Preconditions

- Build succeeds: `dotnet build WinCalendar.sln`
- App launches: `dotnet run --project WinCalendar.App/WinCalendar.App.csproj`
- Test data available (load sample events where useful)

## Modes to Validate

Run the full checklist in each mode:

1. `View -> Themes -> Light`
2. `View -> Themes -> Dark`
3. `View -> Themes -> System` (verify it matches current Windows app mode)

## Core Acceptance Criteria

- No hardcoded light surfaces appear when Dark mode is active.
- Text contrast remains readable for primary and secondary text.
- Disabled states remain legible and distinct.
- Hover/selection states are visible but not overpowering.
- Inputs, popups, and list/table controls honour the active theme.
- Theme switch applies immediately and remains after restart.

## Visual Checklist by Surface

### 1) Main Window Shell

- [ ] Menu bar background/foreground align with active mode.
- [ ] Top-level menu items no longer appear as segmented buttons.
- [ ] Submenu popups honour mode colours (background, text, hover, disabled).
- [ ] Command bands/buttons/read-only labels use themed colours.
- [ ] Loading/error/empty overlays are readable in both modes.
- [ ] Right sidebar panel and calendar surface honour mode colours.

### 2) Calendar Views

- [ ] Day view grid and row text colours are readable.
- [ ] Week view cards/lists honour panel, border, and secondary text tokens.
- [ ] Work Week view cards/lists honour panel, border, and secondary text tokens.
- [ ] Month cell borders/backgrounds honour mode.
- [ ] Month “today” accent border is visible in both modes.
- [ ] Month in-range/out-of-range day labels remain clearly distinguishable.

### 3) Event Editor Dialog

- [ ] Window background/foreground follow active theme.
- [ ] All inputs (`TextBox`, `ComboBox`, `DatePicker`) honour themed background, border, and text.
- [ ] Disabled time fields are visibly disabled but still legible.
- [ ] Recurrence controls and builder actions render consistently.
- [ ] Exceptions/Notes editors match input theme colours.
- [ ] Footer action buttons (Delete/Cancel/Save) remain readable.

### 4) Search Events Dialog

- [ ] Search input/button honour theme.
- [ ] Data grid background, headers, cells, and row selection honour theme.
- [ ] Empty/error/loading state text colours honour theme.

### 5) Category Manager Dialog

- [ ] Top input row honours theme.
- [ ] Data grid body/header/selection honours theme.
- [ ] No dark-on-dark or light-on-light text in any state.

### 6) Template Manager Dialog

- [ ] Input controls and Add button honour theme.
- [ ] Data grid body/header/selection honours theme.
- [ ] No mismatched popup/list colours when editing fields.

### 7) Countdown Manager Dialog

- [ ] Input controls and action buttons honour theme.
- [ ] Data grid body/header/selection honours theme.
- [ ] Validation message uses themed error colour and remains readable.

## Behaviour/State Checks

- [ ] Theme persists across app restart.
- [ ] Sidebar visibility setting persists across app restart.
- [ ] Switching theme while dialogs are open does not cause unreadable controls.

## Sign-off Record

### Run Metadata

- Date:
- Tester:
- Windows mode at start:
- Branch:
- Commit:

### Result Summary

- Light: Pass / Fail
- Dark: Pass / Fail
- System: Pass / Fail

### Defects Found

- ID:
- Surface:
- Mode:
- Screenshot:
- Notes:

### Final Decision

- [ ] Approved for custom theme work
- [ ] Requires additional fixes
