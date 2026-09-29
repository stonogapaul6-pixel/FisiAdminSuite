using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using FisiAdmin.Domain;

namespace FisiAdmin.Desktop;
public partial class MainWindow : Window
{
    private readonly HttpClient _api = new() { BaseAddress = new Uri("http://localhost:5080") };
    public MainWindow() { InitializeComponent(); var login = Session.Current ?? throw new InvalidOperationException("Keine Sitzung"); _api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login.AccessToken); IdentityText.Text = $"{login.DisplayName} · {string.Join(", ", login.Roles)}"; Loaded += async (_, _) => await LoadDashboard(); }
    private async void Logout_Click(object sender, RoutedEventArgs e) { try { await _api.PostAsync("/api/v1/auth/logout", null); } catch { } Session.Set(null!); new LoginWindow().Show(); Close(); }
    private async void Dashboard_Click(object sender, RoutedEventArgs e) => await LoadDashboard();
    private async void Users_Click(object sender, RoutedEventArgs e) => await LoadUsers();
    private async void Refresh_Click(object sender, RoutedEventArgs e) { if (PageTitle.Text == "Benutzer") await LoadUsers(); else await LoadDashboard(); }

    private async Task LoadDashboard()
    {
        PageTitle.Text = "Dashboard"; Metrics.Children.Clear();
        try {
            var d = await _api.GetFromJsonAsync<DashboardSummary>("/api/v1/dashboard");
            if (d is null) return;
            AddMetric("Server online", $"{d.ServersOnline} / {d.ServersTotal}"); AddMetric("Aktive Benutzer", d.ActiveUsers.ToString()); AddMetric("Offene Jobs", d.OpenJobs.ToString()); AddMetric("Security Score", $"{d.SecurityScore}%");
            MainGrid.ItemsSource = await _api.GetFromJsonAsync<List<ServerStatus>>("/api/v1/servers"); StatusText.Text = "Live-Demo über ASP.NET Core API";
        } catch (Exception ex) { StatusText.Text = "API nicht erreichbar: " + ex.Message; }
    }
    private async Task LoadUsers()
    {
        PageTitle.Text = "Benutzer"; Metrics.Children.Clear();
        try { MainGrid.ItemsSource = await _api.GetFromJsonAsync<List<DirectoryUser>>("/api/v1/users?search=" + Uri.EscapeDataString(SearchBox.Text)); StatusText.Text = "Active-Directory-Demodaten"; }
        catch (Exception ex) { StatusText.Text = "API nicht erreichbar: " + ex.Message; }
    }
    private void AddMetric(string title, string value)
    {
        var panel = new StackPanel { Margin = new Thickness(8) };
        panel.Children.Add(new TextBlock { Text = title, Foreground = System.Windows.Media.Brushes.Gray });
        panel.Children.Add(new TextBlock { Text = value, FontSize = 26, FontWeight = FontWeights.SemiBold });
        Metrics.Children.Add(new Border { Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 23, 34)), CornerRadius = new CornerRadius(12), Padding = new Thickness(18), Child = panel });
    }
}
