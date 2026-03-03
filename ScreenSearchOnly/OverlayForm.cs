using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenSearchOnly;

internal enum ClickMode
{
    Left,
    Right
}

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
        Shown += (_, _) => Focus();
    }

    private void HandlePaint(object? sender, PaintEventArgs e)
    {
        using var backgroundBrush = new SolidBrush(Color.FromArgb(220, 20, 20, 20));
        using var accentBrush = new SolidBrush(Color.FromArgb(255, 0, 120, 212));
        using var font = new Font("Segoe UI", 10f, FontStyle.Bold);

        foreach (var target in _targets.Values)
        {
            var b = target.Bounds;
            if (b.Width <= 0 || b.Height <= 0)
            {
                continue;
            }

            var markerWidth = Math.Max(28, target.Label.Length * 12);
            var markerX = b.Left + (b.Width / 2) - (markerWidth / 2);
            var markerY = b.Bottom + 4;
            var marker = new Rectangle(markerX, markerY, markerWidth, 22);
            e.Graphics.FillRectangle(backgroundBrush, marker);
            e.Graphics.DrawRectangle(Pens.White, marker);
            e.Graphics.DrawString(target.Label, font, accentBrush, marker.Left + 4, marker.Top + 3);
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
            return;
        }

        if (e.KeyCode == Keys.Back && _buffer.Length > 0)
        {
            _buffer = _buffer[..^1];
            return;
        }

        var keyChar = KeyToChar(e.KeyCode);
        if (keyChar == null)
        {
            return;
        }

        _buffer += keyChar;

        var matching = _targets.Keys
            .Where(x => x.StartsWith(_buffer, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matching.Count == 0)
        {
            _buffer = string.Empty;
            return;
        }

        if (!_targets.TryGetValue(_buffer, out var exact))
        {
            return;
        }

        var hasLongerMatch = matching.Any(x => x.Length > _buffer.Length);
        if (hasLongerMatch)
        {
            return;
        }

        var clickMode = e.Shift ? ClickMode.Right : ClickMode.Left;
        PerformClick(exact.Bounds, clickMode);
        Close();
    }

    private static char? KeyToChar(Keys keyCode)
    {
        if (keyCode is >= Keys.A and <= Keys.Z)
        {
            return (char)('A' + (keyCode - Keys.A));
        }

        return null;
    }

    private void PerformClick(Rectangle bounds, ClickMode clickMode)
    {
        var originalPosition = Cursor.Position;
        var x = bounds.Left + (bounds.Width / 2);
        var y = bounds.Top + (bounds.Height / 2);

        Hide();
        Cursor.Position = new Point(x, y);
        Application.DoEvents();

        var downFlag = clickMode == ClickMode.Left ? MouseEventFlags.LeftDown : MouseEventFlags.RightDown;
        var upFlag = clickMode == ClickMode.Left ? MouseEventFlags.LeftUp : MouseEventFlags.RightUp;

        mouse_event((uint)downFlag, 0, 0, 0, UIntPtr.Zero);
        mouse_event((uint)upFlag, 0, 0, 0, UIntPtr.Zero);

        Cursor.Position = originalPosition;
    }

    [Flags]
    private enum MouseEventFlags : uint
    {
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010
    }

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
}
