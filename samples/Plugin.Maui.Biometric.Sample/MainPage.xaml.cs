using Plugin.Maui.Biometric;

namespace Plugin.Maui.Biometric.Sample;

public partial class MainPage : ContentPage
{
    readonly Label log = new() { LineBreakMode = LineBreakMode.WordWrap };

    public MainPage()
    {
        InitializeComponent();
        Root.Children.Add(new Button { Text = "Get availability", Command = new Command(async () => await Run(Availability)) });
        Root.Children.Add(new Button { Text = "Authenticate (PIN allowed)", Command = new Command(async () => await Run(() => Auth(true))) });
        Root.Children.Add(new Button { Text = "Authenticate (biometric only)", Command = new Command(async () => await Run(() => Auth(false))) });
        Root.Children.Add(log);
    }

    async Task Availability()
    {
        var value = await Biometric.Current.GetAvailabilityAsync();
        log.Text = $"Availability: {value}";
    }

    async Task Auth(bool pin)
    {
        var result = await Biometric.Current.AuthenticateAsync(new BiometricRequest
        {
            Reason = pin ? "Unlock payroll" : "Biometric only",
            AllowDeviceCredential = pin,
            CancelTitle = "Cancel"
        });
        log.Text = $"{result.Status} {result.Message}";
    }

    async Task Run(Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) { log.Text = ex.Message; }
    }
}
