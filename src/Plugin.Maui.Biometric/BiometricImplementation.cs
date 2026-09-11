namespace Plugin.Maui.Biometric;

sealed class BiometricImplementation : IBiometric
{
    readonly IBiometricAuthenticator authenticator;

    public BiometricImplementation(BiometricOptions options, IBiometricAuthenticator authenticator)
    {
        Options = options;
        this.authenticator = authenticator;
    }

    public BiometricOptions Options { get; }

    public void Configure(Action<BiometricOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Options);
    }

    public Task<BiometricAvailability> GetAvailabilityAsync(CancellationToken cancellationToken = default) =>
        authenticator.GetAvailabilityAsync(Options, cancellationToken);

    public Task<BiometricResult> AuthenticateAsync(BiometricRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new BiometricRequest();
        if (string.IsNullOrWhiteSpace(request.Reason))
            request.Reason = Options.DefaultReason;
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BiometricException("Reason is required.");
        request.AllowDeviceCredential ??= Options.AllowDeviceCredential;
        return authenticator.AuthenticateAsync(request, Options, cancellationToken);
    }
}
