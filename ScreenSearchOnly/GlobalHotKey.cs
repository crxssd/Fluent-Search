using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenSearchOnly;

[Flags]
internal enum Modifiers
{
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008
}

internal sealed class GlobalHotKey : NativeWindow, IDisposable
{
    private const int WmHotKey = 0x0312;
    private readonly int _id;

    public event EventHandler? HotKeyPressed;

    public GlobalHotKey(Modifiers modifiers, Keys key)
    {
        _id = GetHashCode();
        CreateHandle(new CreateParams());

        if (!RegisterHotKey(Handle, _id, (uint)modifiers, (uint)key))
        {
            throw new InvalidOperationException("Failed to register global hotkey.");
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotKey && m.WParam.ToInt32() == _id)
        {
            HotKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        UnregisterHotKey(Handle, _id);
        DestroyHandle();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
