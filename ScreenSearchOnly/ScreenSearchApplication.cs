using System.Drawing;
using System.Windows.Automation;
using System.Windows.Forms;

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

        var elements = AutomationScanner.ScanClickableElements();
        if (elements.Count == 0)
        {
            _trayIcon.ShowBalloonTip(2000, "Screen Search", "No clickable elements found.", ToolTipIcon.Info);
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
    public static IReadOnlyList<ScreenTarget> ScanClickableElements()
    {
        var result = new List<ScreenTarget>();
        var root = AutomationElement.RootElement;
        if (root == null)
        {
            return result;
        }

        var condition = new OrCondition(
            new PropertyCondition(AutomationElement.IsInvokePatternAvailableProperty, true),
            new PropertyCondition(AutomationElement.IsSelectionItemPatternAvailableProperty, true),
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Hyperlink),
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));

        var all = root.FindAll(TreeScope.Subtree, condition);
        var labels = KeyLabelGenerator.Generate(all.Count);

        for (var i = 0; i < all.Count; i++)
        {
            var element = all[i];
            var rect = element.Current.BoundingRectangle;
            if (rect.Width < 4 || rect.Height < 4)
            {
                continue;
            }

            if (rect.Right < 0 || rect.Bottom < 0)
            {
                continue;
            }

            var name = element.Current.Name;
            var label = labels[i];
            result.Add(new ScreenTarget(label, name, Rectangle.Round(new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height)), element));
        }

        return result;
    }
}

internal sealed record ScreenTarget(string Label, string Name, Rectangle Bounds, AutomationElement Element);
