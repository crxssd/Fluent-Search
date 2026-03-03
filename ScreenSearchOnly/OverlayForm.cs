using System.Drawing;
using System.Runtime.InteropServices;
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

        var hasAnyMatch = _targets.Keys.Any(x => x.StartsWith(_buffer, StringComparison.OrdinalIgnoreCase));
        if (!hasAnyMatch)
        {
            _buffer = string.Empty;
            return;
        }

        if (_targets.TryGetValue(_buffer, out var exact))
        {
            ClickTarget(exact.Bounds);
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

    private static void ClickTarget(Rectangle bounds)
    {
        var x = bounds.Left + (bounds.Width / 2);
        var y = bounds.Top + (bounds.Height / 2);

        Cursor.Position = new Point(x, y);
        mouse_event(0x0002 | 0x0004, 0, 0, 0, UIntPtr.Zero);
    }

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
}
