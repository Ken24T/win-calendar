# WinCalendar Operational Troubleshooting

Last updated: 2026-02-14

## Startup and Runtime Failures

If WinCalendar fails on startup or shows an unexpected runtime error, use this quick flow:

1. Check diagnostics log path:
   - `%LOCALAPPDATA%\\WinCalendar\\logs\\startup.log`
2. Review the latest entries first, then archived `startup-*.log` files if needed.
3. Look for the first `ERROR` entry in the failing run and note:
   - exception type
   - message
   - top stack frame in WinCalendar code
4. Re-run the app once after collecting details to confirm reproducibility.

## Note on Historical Errors

Diagnostics logs are append-only between rollovers, so older resolved errors can remain visible in the same file. Prioritise the most recent run entries when triaging current issues.

## Common Non-Critical Noise

`OperationCanceledException` and `TaskCanceledException` can occur during shutdown or cancelled UI tasks. These are treated as non-critical and are now suppressed from error-level diagnostics to reduce noise.
