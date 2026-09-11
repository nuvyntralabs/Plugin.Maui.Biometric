#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Biometric;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using Java.Lang;

namespace Plugin.Maui.Biometric;

sealed class PlatformAuthenticator : IBiometricAuthenticator
{
    public static IBiometricAuthenticator Create() => new PlatformAuthenticator();

    public Task<BiometricAvailability> GetAvailabilityAsync(BiometricOptions options, CancellationToken cancellationToken = default)
    {
        var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
                      ?? Microsoft.Maui.ApplicationModel.Platform.AppContext;
        if (context is null)
            return Task.FromResult(BiometricAvailability.NotAvailable);

        var result = BiometricManager.From(context).CanAuthenticate(ResolveAuthenticators(options, allowPin: options.AllowDeviceCredential));
        return Task.FromResult(result switch
        {
            BiometricManager.BiometricSuccess => BiometricAvailability.Available,
            BiometricManager.BiometricErrorNoneEnrolled => options.AllowDeviceCredential && DeviceSecure(context)
                ? BiometricAvailability.Available
                : BiometricAvailability.NotEnrolled,
            BiometricManager.BiometricErrorNoHardware => options.AllowDeviceCredential && DeviceSecure(context)
                ? BiometricAvailability.Available
                : BiometricAvailability.NotAvailable,
            _ => BiometricAvailability.Unknown
        });
    }

    public Task<BiometricResult> AuthenticateAsync(BiometricRequest request, BiometricOptions options, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<BiometricResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is not FragmentActivity activity)
                {
                    tcs.TrySetResult(BiometricResult.Fail(BiometricStatus.NotAvailable, "A foreground activity is required."));
                    return;
                }

                var callback = new AuthCallback(tcs);
                var executor = ContextCompat.GetMainExecutor(activity)
                    ?? throw new BiometricException("No main executor is available.");
                var prompt = new BiometricPrompt(activity, executor, callback);
                if (cancellationToken.CanBeCanceled)
                    cancellationToken.Register(() => { prompt.CancelAuthentication(); tcs.TrySetCanceled(cancellationToken); });
                prompt.Authenticate(BuildPrompt(request, options));
            }
            catch (System.Exception ex)
            {
                tcs.TrySetResult(BiometricResult.Fail(BiometricStatus.NotAvailable, ex.Message));
            }
        });
        return tcs.Task;
    }

    static BiometricPrompt.PromptInfo BuildPrompt(BiometricRequest request, BiometricOptions options)
    {
        var allowPin = request.AllowDeviceCredential ?? options.AllowDeviceCredential;
        var builder = new BiometricPrompt.PromptInfo.Builder()
            .SetTitle("Authenticate")
            .SetSubtitle(request.Reason)
            .SetDescription(request.Reason);
        var sdk = (int)Build.VERSION.SdkInt;
        if (allowPin && sdk < (int)BuildVersionCodes.R)
        {
#pragma warning disable CS0618
            builder.SetDeviceCredentialAllowed(true);
#pragma warning restore CS0618
        }
        else
        {
            builder.SetAllowedAuthenticators(ResolveAuthenticators(options, allowPin));
            if (!allowPin)
                builder.SetNegativeButtonText(request.CancelTitle);
        }
        return builder.Build();
    }

    static int ResolveAuthenticators(BiometricOptions options, bool allowPin)
    {
        var flags = BiometricManager.Authenticators.BiometricStrong | BiometricManager.Authenticators.BiometricWeak;
        if (allowPin)
            flags |= BiometricManager.Authenticators.DeviceCredential;
        return flags;
    }

    static bool DeviceSecure(Context context) =>
        (context.GetSystemService(Context.KeyguardService) as KeyguardManager)?.IsDeviceSecure == true;

    sealed class AuthCallback : BiometricPrompt.AuthenticationCallback
    {
        readonly TaskCompletionSource<BiometricResult> tcs;
        public AuthCallback(TaskCompletionSource<BiometricResult> tcs) => this.tcs = tcs;
        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result) =>
            tcs.TrySetResult(BiometricResult.Ok());
        public override void OnAuthenticationFailed() { }
        public override void OnAuthenticationError(int errorCode, ICharSequence errString)
        {
            var message = errString?.ToString();
            var status = errorCode switch
            {
                BiometricPrompt.ErrorUserCanceled or BiometricPrompt.ErrorNegativeButton or BiometricPrompt.ErrorCanceled => BiometricStatus.Canceled,
                BiometricPrompt.ErrorLockout or BiometricPrompt.ErrorLockoutPermanent => BiometricStatus.LockedOut,
                BiometricPrompt.ErrorHwNotPresent or BiometricPrompt.ErrorHwUnavailable => BiometricStatus.NotAvailable,
                BiometricPrompt.ErrorNoBiometrics or BiometricPrompt.ErrorNoDeviceCredential => BiometricStatus.NotEnrolled,
                _ => BiometricStatus.Failed
            };
            tcs.TrySetResult(BiometricResult.Fail(status, message));
        }
    }
}
#endif
