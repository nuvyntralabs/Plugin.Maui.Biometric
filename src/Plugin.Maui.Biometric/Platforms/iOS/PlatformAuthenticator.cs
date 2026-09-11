#if IOS
using LocalAuthentication;

namespace Plugin.Maui.Biometric;

sealed class PlatformAuthenticator : IBiometricAuthenticator
{
    public static IBiometricAuthenticator Create() => new PlatformAuthenticator();

    public Task<BiometricAvailability> GetAvailabilityAsync(BiometricOptions options, CancellationToken cancellationToken = default)
    {
        using var context = new LAContext();
        var policy = options.AllowDeviceCredential
            ? LAPolicy.DeviceOwnerAuthentication
            : LAPolicy.DeviceOwnerAuthenticationWithBiometrics;
        if (context.CanEvaluatePolicy(policy, out var error))
            return Task.FromResult(BiometricAvailability.Available);
        if (error is null)
            return Task.FromResult(BiometricAvailability.Unknown);
        return Task.FromResult(error.Code == (int)LAStatus.BiometryNotEnrolled
            ? BiometricAvailability.NotEnrolled
            : BiometricAvailability.NotAvailable);
    }

    public Task<BiometricResult> AuthenticateAsync(BiometricRequest request, BiometricOptions options, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<BiometricResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var context = new LAContext();
            var allowPin = request.AllowDeviceCredential ?? options.AllowDeviceCredential;
            var policy = allowPin ? LAPolicy.DeviceOwnerAuthentication : LAPolicy.DeviceOwnerAuthenticationWithBiometrics;
            if (!context.CanEvaluatePolicy(policy, out var error))
            {
                tcs.TrySetResult(BiometricResult.Fail(
                    error?.Code == (int)LAStatus.BiometryNotEnrolled ? BiometricStatus.NotEnrolled : BiometricStatus.NotAvailable,
                    error?.LocalizedDescription));
                return;
            }

            context.EvaluatePolicy(policy, request.Reason, (success, evalError) =>
            {
                if (success)
                {
                    tcs.TrySetResult(BiometricResult.Ok());
                    return;
                }

                var status = evalError?.Code switch
                {
                    (int)LAStatus.UserCancel or (int)LAStatus.AppCancel or (int)LAStatus.SystemCancel => BiometricStatus.Canceled,
                    (int)LAStatus.BiometryLockout => BiometricStatus.LockedOut,
                    (int)LAStatus.BiometryNotEnrolled => BiometricStatus.NotEnrolled,
                    (int)LAStatus.BiometryNotAvailable => BiometricStatus.NotAvailable,
                    _ => BiometricStatus.Failed
                };
                tcs.TrySetResult(BiometricResult.Fail(status, evalError?.LocalizedDescription));
            });
        });
        return tcs.Task;
    }
}
#endif
