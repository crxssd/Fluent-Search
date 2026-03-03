using System.Drawing;
using System.Windows.Automation;
using System.Windows.Forms;

namespace ScreenSearchOnly;

internal sealed class OverlayForm : Form
{
    private readonly Dictionary<string, ScreenTarget> _targets;
    private string _buffer = string.Empty;

    public OverlayForm(IReadOnlyList<ScreenTarget> targets)
    {
        _targets = targets.ToDictionary(t => t.Label, StringComparer.OrdinalIgnoreCase);

        BackColor = Color.Black;
        Opacity = 0.22;
        ShowInTaskbar = false;
        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        KeyPreview = true;
        StartPosition = FormStartPosition.Manual;
        Bounds = SystemInformation.VirtualScreen;

        KeyDown += HandleKeyDown;
        Paint += HandlePaint;
    }

    private void HandlePaint(object? sender, PaintEventArgs e)
    {
        using var backgroundBrush = new SolidBrush(Color.FromArgb(220, 20, 20, 20));
        using var textBrush = new SolidBrush(Color.White);
        using var accentBrush = new SolidBrush(Color.FromArgb(255, 0, 120, 212));
        using var font = new Font("Segoe UI", 10f, FontStyle.Bold);

        foreach (var target in _targets.Values)
        {
            var b = target.Bounds;
            if (b.Width <= 0 || b.Height <= 0)
            {
                continue;
            }

            var marker = new Rectangle(b.Left, b.Top, Math.Max(28, target.Label.Length * 12), 22);
            e.Graphics.FillRectangle(backgroundBrush, marker);
            e.Graphics.DrawRectangle(Pens.White, marker);
            e.Graphics.DrawString(target.Label, font, accentBrush, marker.Left + 4, marker.Top + 3);

            if (!string.IsNullOrWhiteSpace(target.Name))
            {
                e.Graphics.DrawString(target.Name, font, textBrush, marker.Right + 6, marker.Top + 3);
            }
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
            return;
        }

        var keyChar = KeyToChar(e.KeyCode);
        if (keyChar == null)
        {
            return;
        }

        _buffer += keyChar;

        var matching = _targets.Keys.Where(x => x.StartsWith(_buffer, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matching.Count == 0)
        {
            _buffer = string.Empty;
            return;
        }

        if (_targets.TryGetValue(_buffer, out var exact))
        {
            InvokeTarget(exact.Element);
            Close();
        }
    }

    private static char? KeyToChar(Keys keyCode)
    {
        if (keyCode is >= Keys.A and <= Keys.Z)
        {
            return (char)('A' + (keyCode - Keys.A));
        }

        return null;
    }

    private static void InvokeTarget(AutomationElement element)
    {
        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern))
        {
            ((InvokePattern)pattern).Invoke();
            return;
        }

        if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
        {
            ((SelectionItemPattern)pattern).Select();
            return;
        }

        var rect = element.Current.BoundingRectangle;
        Cursor.Position = new Point((int)(rect.Left + rect.Width / 2), (int)(rect.Top + rect.Height / 2));
        mouse_event(0x0002 | 0x0004, 0, 0, 0, UIntPtr.Zero);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
}
