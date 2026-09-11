namespace Plugin.Maui.Biometric;

#if !ANDROID && !IOS
sealed class PlatformAuthenticator : IBiometricAuthenticator
{
    public static IBiometricAuthenticator Create() => new PlatformAuthenticator();

    public Task<BiometricAvailability> GetAvailabilityAsync(BiometricOptions options, CancellationToken cancellationToken = default) =>
        Task.FromResult(BiometricAvailability.NotSupported);

    public Task<BiometricResult> AuthenticateAsync(BiometricRequest request, BiometricOptions options, CancellationToken cancellationToken = default) =>
        Task.FromResult(BiometricResult.Fail(BiometricStatus.NotSupported, "Biometric is not supported on this target."));
}
#endif
