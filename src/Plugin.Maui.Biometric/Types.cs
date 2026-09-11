namespace Plugin.Maui.Biometric;

public enum BiometricAvailability
{
    Available,
    NotEnrolled,
    NotAvailable,
    Unknown,
    NotSupported
}

public enum BiometricStatus
{
    Succeeded,
    Canceled,
    Failed,
    NotEnrolled,
    NotAvailable,
    LockedOut,
    NotSupported
}

public sealed class BiometricOptions
{
    public bool AllowDeviceCredential { get; set; } = true;
    public string DefaultReason { get; set; } = "Confirm it is you";
}

public sealed class BiometricRequest
{
    public string Reason { get; set; } = "";
    public bool? AllowDeviceCredential { get; set; }
    public string CancelTitle { get; set; } = "Cancel";
}

public sealed class BiometricResult
{
    public BiometricStatus Status { get; init; }
    public string? Message { get; init; }
    public bool Succeeded => Status == BiometricStatus.Succeeded;

    public static BiometricResult Ok() => new() { Status = BiometricStatus.Succeeded };
    public static BiometricResult Fail(BiometricStatus status, string? message = null) =>
        new() { Status = status, Message = message };
}

public sealed class BiometricException : Exception
{
    public BiometricException(string message) : base(message) { }
}

public interface IBiometricAuthenticator
{
    Task<BiometricAvailability> GetAvailabilityAsync(BiometricOptions options, CancellationToken cancellationToken = default);
    Task<BiometricResult> AuthenticateAsync(BiometricRequest request, BiometricOptions options, CancellationToken cancellationToken = default);
}
