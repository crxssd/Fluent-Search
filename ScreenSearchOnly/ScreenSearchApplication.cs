using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace ScreenSearchOnly;

internal sealed class ScreenSearchApplication : IDisposable
{
    private readonly GlobalHotKey _hotKey;
    private readonly NotifyIcon _trayIcon;
    private OverlayForm? _overlay;

    public ScreenSearchApplication()
    {
        _hotKey = new GlobalHotKey(Modifiers.Control | Modifiers.Shift, Keys.Space);
        _hotKey.HotKeyPressed += (_, _) => ToggleOverlay();

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "Screen Search Only",
            Visible = true,
            ContextMenuStrip = BuildContextMenu()
        };

        _trayIcon.DoubleClick += (_, _) => ToggleOverlay();
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Toggle Screen Search", null, (_, _) => ToggleOverlay());
        menu.Items.Add("Exit", null, (_, _) => Application.Exit());
        return menu;
    }

    private void ToggleOverlay()
    {
        if (_overlay is { IsDisposed: false })
        {
            _overlay.Close();
            _overlay = null;
            return;
        }

        var elements = AutomationScanner.ScanClickableElementsInForegroundWindow();
        if (elements.Count == 0)
        {
            return;
        }

        _overlay = new OverlayForm(elements);
        _overlay.FormClosed += (_, _) => _overlay = null;
        _overlay.Show();
        _overlay.Activate();
    }

    public void Dispose()
    {
        _overlay?.Dispose();
        _hotKey.Dispose();
        _trayIcon.Dispose();
    }
}

internal static class AutomationScanner
{
    public static IReadOnlyList<ScreenTarget> ScanClickableElementsInForegroundWindow()
    {
        var result = new List<ScreenTarget>();
        var windowHandle = GetForegroundWindow();
        if (windowHandle == IntPtr.Zero || !GetWindowRect(windowHandle, out var windowRect))
        {
            return result;
        }

        using var automation = new UIA3Automation();
        var window = automation.FromHandle(windowHandle);
        if (window == null)
        {
            return result;
        }

        var cf = automation.ConditionFactory;
        var clickableCondition = cf.ByControlType(ControlType.Button)
            .Or(cf.ByControlType(ControlType.Hyperlink))
            .Or(cf.ByControlType(ControlType.MenuItem))
            .Or(cf.ByControlType(ControlType.TabItem))
            .Or(cf.ByControlType(ControlType.CheckBox))
            .Or(cf.ByControlType(ControlType.RadioButton));

        var all = window.FindAllDescendants(clickableCondition);
        var labels = KeyLabelGenerator.Generate(all.Length);

        for (var i = 0; i < all.Length; i++)
        {
            var element = all[i];
            var rect = element.BoundingRectangle;

            if (rect.IsEmpty || rect.Width < 4 || rect.Height < 4)
            {
                continue;
            }

            if (!IntersectsWindow(rect.Left, rect.Top, rect.Right, rect.Bottom, windowRect))
            {
                continue;
            }

            var bounds = new Rectangle((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
            var name = element.Name;
            var label = labels[i];
            result.Add(new ScreenTarget(label, name ?? string.Empty, bounds));
        }

        return result;
    }

    private static bool IntersectsWindow(double left, double top, double right, double bottom, Rect windowRect)
    {
        return right > windowRect.Left
            && left < windowRect.Right
            && bottom > windowRect.Top
            && top < windowRect.Bottom;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}

internal sealed record ScreenTarget(string Label, string Name, Rectangle Bounds);
