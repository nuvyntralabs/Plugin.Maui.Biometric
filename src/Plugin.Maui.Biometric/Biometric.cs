namespace Plugin.Maui.Biometric;

public static class Biometric
{
    static IBiometric? current;

    public static IBiometric Current =>
        current ?? throw new InvalidOperationException("Biometric is not initialized. Call builder.UseBiometric() or Biometric.SetDefault().");

    public static void SetDefault(IBiometric implementation) =>
        current = implementation ?? throw new ArgumentNullException(nameof(implementation));

    public static IBiometric Create(BiometricOptions? options = null, IBiometricAuthenticator? authenticator = null)
    {
        var instance = new BiometricImplementation(options ?? new BiometricOptions(), authenticator ?? PlatformAuthenticator.Create());
        SetDefault(instance);
        return instance;
    }
}
