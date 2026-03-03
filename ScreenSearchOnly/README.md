# Screen Search Only (Standalone)

This folder contains a lightweight standalone implementation of Fluent Search's **screen search** interaction model, without launcher/search box functionality.

## What it does

- Registers a global hotkey: `Ctrl + Shift + Space`.
- Scans on-screen UI Automation elements that look clickable.
- Draws an always-on-top transparent overlay with key labels.
- Lets you type a label to invoke/select the associated element.
- Closes on `Esc`.

## Build and run

```bash
dotnet run --project ScreenSearchOnly/ScreenSearchOnly.csproj
```

> Requires Windows 10/11 because it uses Windows UI Automation + WinForms.

## Notes

- This is intentionally focused only on the screen interaction flow.
- No Fluent Search launcher, indexing, plugin, or command palette logic is included.
