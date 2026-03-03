# Screen Search Only (Standalone)

This folder contains a lightweight standalone implementation of Fluent Search's **screen search** interaction model, without launcher/search box functionality.

## What it does

- Registers a global hotkey: `Ctrl + Shift + Space`.
- Scans **only the currently focused window** for clickable controls.
- Draws an always-on-top transparent overlay with key labels displayed below each target.
- Lets you type a label to click the associated element.
- Supports right-click activation by holding `Shift` when pressing the final key in a label.
- Closes on `Esc`.

## Label behavior

- If there are up to 26 targets, single-letter labels are used.
- If there are more than 26 targets, labels switch to two-letter combinations only.
- This avoids ambiguous behavior where a single-letter label would conflict with a two-letter label sharing the same prefix.

## Build and run

```bash
dotnet run --project ScreenSearchOnly/ScreenSearchOnly.csproj
```

> Requires Windows 10/11.

## Notes

- Uses `FlaUI.UIA3` for screen element discovery.
- This is intentionally focused only on the screen interaction flow.
- No Fluent Search launcher, indexing, plugin, or command palette logic is included.
