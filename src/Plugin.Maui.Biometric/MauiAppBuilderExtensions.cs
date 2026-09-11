namespace Plugin.Maui.Biometric;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseBiometric(this MauiAppBuilder builder, Action<BiometricOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var options = new BiometricOptions();
        configure?.Invoke(options);
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton(Biometric.Create(options));
        return builder;
    }
}
