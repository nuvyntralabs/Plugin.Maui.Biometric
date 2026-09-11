using Microsoft.Extensions.Logging;
using Plugin.Maui.Biometric;

namespace Plugin.Maui.Biometric.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Services.AddSingleton<MainPage>();
        builder.UseMauiApp<App>()
            .UseBiometric(o => { o.AllowDeviceCredential = true; o.DefaultReason = "Unlock sample"; });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
