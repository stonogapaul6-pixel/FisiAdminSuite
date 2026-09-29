using System.Net.Http.Json;
using System.Windows;
using FisiAdmin.Domain;
namespace FisiAdmin.Desktop;
public partial class LoginWindow : Window
{
    private readonly HttpClient _api = new() { BaseAddress = new Uri("http://localhost:5080") };
    public LoginWindow() => InitializeComponent();
    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = "";
        try
        {
            var response = await _api.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(UserNameBox.Text.Trim(), PasswordBox.Password));
            if (!response.IsSuccessStatusCode) { ErrorText.Text = "Benutzername oder Passwort ist falsch."; return; }
            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (login is null) return;
            Session.Set(login);
            new MainWindow().Show(); Close();
        }
        catch (Exception ex) { ErrorText.Text = "API nicht erreichbar: " + ex.Message; }
    }
}
public static class Session
{
    public static LoginResponse? Current { get; private set; }
    public static void Set(LoginResponse login) => Current = login;
    public static bool IsInRole(string role) => Current?.Roles.Contains(role) == true;
}
