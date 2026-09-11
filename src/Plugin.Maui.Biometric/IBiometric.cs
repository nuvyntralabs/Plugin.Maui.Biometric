namespace Plugin.Maui.Biometric;

public interface IBiometric
{
    BiometricOptions Options { get; }
    Task<BiometricAvailability> GetAvailabilityAsync(CancellationToken cancellationToken = default);
    Task<BiometricResult> AuthenticateAsync(BiometricRequest? request = null, CancellationToken cancellationToken = default);
    void Configure(Action<BiometricOptions> configure);
}
