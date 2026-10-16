using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CatLaser;

/// <summary>Generate an authentic PNG of the actual WPF settings window on Windows.</summary>
internal static class CaptureSettings
{
    public static void Run(string destination)
    {
        var app = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.Startup += (_, _) =>
        {
            var controller = new LaserController();
            var hotkeys = new HotkeyWindow(() => { }, () => { }, () => { },
                () => { }, () => { }, () => { });
            var settings = new SettingsWindow(controller, () => app.Shutdown(), hotkeys);
            settings.Show();
            settings.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                try
                {
                    settings.UpdateLayout();
                    int width = Math.Max(1, (int)Math.Ceiling(settings.ActualWidth));
                    int height = Math.Max(1, (int)Math.Ceiling(settings.ActualHeight));
                    var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(settings);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
                    using var output = File.Create(destination);
                    encoder.Save(output);
                    if (output.Length < 20000)
                        throw new InvalidOperationException("WPF capture appears unexpectedly small.");
                }
                finally
                {
                    settings.Close();
                    hotkeys.Dispose();
                    controller.Dispose();
                    app.Shutdown();
                }
            }));
        };
        app.Run();
    }
}
