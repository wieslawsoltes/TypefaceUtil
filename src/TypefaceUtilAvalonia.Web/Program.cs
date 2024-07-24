using System.Runtime.Versioning;
using Avalonia;
using TypefaceUtil.Avalonia;

[assembly:SupportedOSPlatform("browser")]

internal class Program
{
    private static void Main(string[] args) 
        => BuildAvaloniaApp().SetupBrowserApp("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
