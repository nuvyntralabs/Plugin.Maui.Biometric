using Plugin.Maui.Biometric;

namespace Plugin.Maui.Biometric.Tests;

sealed class FakeAuth : IBiometricAuthenticator
{
    public BiometricAvailability Availability { get; set; } = BiometricAvailability.Available;
    public BiometricResult Result { get; set; } = BiometricResult.Ok();
    public int AuthCalls { get; private set; }
    public BiometricRequest? LastRequest { get; private set; }

    public Task<BiometricAvailability> GetAvailabilityAsync(BiometricOptions options, CancellationToken cancellationToken = default) =>
        Task.FromResult(Availability);

    public Task<BiometricResult> AuthenticateAsync(BiometricRequest request, BiometricOptions options, CancellationToken cancellationToken = default)
    {
        AuthCalls++;
        LastRequest = request;
        return Task.FromResult(Result);
    }
}

public sealed class BiometricTests
{
    [Fact]
    public async Task Net10_platform_reports_not_supported()
    {
        var api = Biometric.Create(new BiometricOptions());
        Assert.Equal(BiometricAvailability.NotSupported, await api.GetAvailabilityAsync());
        var result = await api.AuthenticateAsync(new BiometricRequest { Reason = "test" });
        Assert.Equal(BiometricStatus.NotSupported, result.Status);
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData(BiometricStatus.Succeeded, true)]
    [InlineData(BiometricStatus.Canceled, false)]
    [InlineData(BiometricStatus.Failed, false)]
    [InlineData(BiometricStatus.NotEnrolled, false)]
    [InlineData(BiometricStatus.NotAvailable, false)]
    [InlineData(BiometricStatus.LockedOut, false)]
    public async Task Authenticate_maps_every_status(BiometricStatus status, bool ok)
    {
        var auth = new FakeAuth { Result = status == BiometricStatus.Succeeded ? BiometricResult.Ok() : BiometricResult.Fail(status, status.ToString()) };
        var api = new BiometricImplementation(new BiometricOptions { DefaultReason = "Confirm" }, auth);
        var result = await api.AuthenticateAsync();
        Assert.Equal(status, result.Status);
        Assert.Equal(ok, result.Succeeded);
        Assert.Equal("Confirm", auth.LastRequest!.Reason);
    }

    [Fact]
    public async Task Empty_reason_without_default_throws()
    {
        var api = new BiometricImplementation(new BiometricOptions { DefaultReason = "" }, new FakeAuth());
        await Assert.ThrowsAsync<BiometricException>(() => api.AuthenticateAsync(new BiometricRequest { Reason = "" }));
    }

    [Fact]
    public void Configure_updates_options()
    {
        var api = new BiometricImplementation(new BiometricOptions(), new FakeAuth());
        api.Configure(o => o.AllowDeviceCredential = false);
        Assert.False(api.Options.AllowDeviceCredential);
    }

    [Fact]
    public void UseBiometric_registers_service()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();
        builder.UseBiometric(o => o.DefaultReason = "Pay");
        // builder already has services; also test Create path
        var created = Biometric.Create(new BiometricOptions { DefaultReason = "Pay" }, new FakeAuth());
        Assert.Equal("Pay", created.Options.DefaultReason);
    }
}
