using System.Windows.Forms;

namespace ScreenSearchOnly;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var app = new ScreenSearchApplication();
        Application.Run();
    }
}
