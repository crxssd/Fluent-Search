# Screen Search Only (Standalone)

This folder contains a lightweight standalone implementation of Fluent Search's **screen search** interaction model, without launcher/search box functionality.

## What it does

- Registers a global hotkey: `Ctrl + Shift + Space`.
- Scans visible desktop UI elements that look clickable.
- Draws an always-on-top transparent overlay with key labels.
- Lets you type a label to move/click the associated element.
- Closes on `Esc`.

## Build and run

```bash
dotnet run --project ScreenSearchOnly/ScreenSearchOnly.csproj
```

> Requires Windows 10/11.

## Notes

- Uses `FlaUI.UIA3` for screen element discovery (instead of direct `System.Windows.Automation` references).
- This is intentionally focused only on the screen interaction flow.
- No Fluent Search launcher, indexing, plugin, or command palette logic is included.
