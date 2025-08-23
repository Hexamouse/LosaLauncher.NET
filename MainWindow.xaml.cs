using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace GameLauncher
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly string settingsFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public MainWindow()
        {
            InitializeComponent();
            PasswordBox.Loaded += PasswordBox_Loaded;
            client.Timeout = TimeSpan.FromSeconds(15);

            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    var settings = JsonConvert.DeserializeAnonymousType(json, new { RememberedUser = "" });
                    if (settings != null && !string.IsNullOrEmpty(settings.RememberedUser))
                    {
                        UsernameBox.Text = settings.RememberedUser;
                        RememberMeCheckBox.IsChecked = true;
                        PasswordBox.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new { RememberedUser = (RememberMeCheckBox.IsChecked == true) ? UsernameBox.Text : "" };
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                CustomMessageBox.Show("Login Error", "Please enter your username and password.");
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Content = "Logging in...";

            try
            {
                SaveSettings();

                var payload = new { username = username, password = password };
                string jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (client.DefaultRequestHeaders.Authorization == null)
                {
                    string mainkey = "localsx";
                    string apikey = "sXhADM9lO1fKcD7";
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mainkey + apikey);
                    client.DefaultRequestHeaders.Add("User-Agent", "csharp-launcher/1.0");
                    client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                }

                string url = "http://192.168.1.100:3303/api/sntl_launcher";
                HttpResponseMessage response = await client.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    LoginResponse? loginData = JsonConvert.DeserializeObject<LoginResponse>(responseBody);
                    if (loginData != null && loginData.UserID != null && loginData.EncodeKey != null && loginData.ServerID != null)
                    {
                        //MessageBox.Show($"Login successful! Welcome, {username}.\nUserID: {loginData.UserID}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LaunchGame(loginData.UserID, loginData.EncodeKey, loginData.ServerID);
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        CustomMessageBox.Show("Login Error", "Failed to parse server response.");
                    }
                }
                else
                {
                    string errorMessage = responseBody;
                    try
                    {
                        var errorObject = JsonConvert.DeserializeAnonymousType(responseBody, new { message = "" });
                        if (errorObject != null && !string.IsNullOrEmpty(errorObject.message))
                        {
                            errorMessage = errorObject.message;
                        }
                    }
                    catch { }
                    CustomMessageBox.Show($"Login Failed: {response.ReasonPhrase}", errorMessage);
                }
            }
            catch (TaskCanceledException)
            {
                CustomMessageBox.Show("Request Timeout", "The server did not respond in time. Please check your internet connection and try again.");
            }
            catch (HttpRequestException httpEx)
            {
                CustomMessageBox.Show("Connection Error", $"A network error occurred: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Unexpected Error", $"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Login";
            }
        }

        private void LaunchGame(string userID, string encodeKey, string serverID)
        {
            try
            {
                string encode = SeedEncryptor.Encode15("111111111111111", "111111111111111");
                string encid = SeedEncryptor.Encode15(userID, encodeKey);

                string encodefix = encode;
                string encidfix = encid;
                string gameserverid = serverID;

                string arguments = $"EDEW3940FVDP4950,10,20,30,1,autoupgrade_info.ini,1000,0,1,0,?{encodefix}{encidfix}?0?dade5655e7293d60b6f612c3a2fa7675797b717a06045d678e910689?{gameserverid}?2010,7,15,1?10201?";

                Process.Start("autoupgrade.exe", arguments);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Launch Error", $"An error occurred during game launch preparation: {ex.Message}");
            }
        }

        private void ForgotPasswordLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string forgotPasswordUrl = "http://your-website.com/forgot-password";
                Process.Start(new ProcessStartInfo(forgotPasswordUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", $"Could not open the link: {ex.Message}");
            }
        }

        // ===== METHOD CHECKER SUDAH DIUBAH MENJADI DISCORD =====
        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ganti dengan link invite server Discord Anda
                string discordInviteUrl = "https://discord.gg/pdwp89Kgwe";
                Process.Start(new ProcessStartInfo(discordInviteUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", $"Could not open the link: {ex.Message}");
            }
        }
        // =======================================================

        #region UI Logic

        private void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(sender as PasswordBox);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(sender as PasswordBox);
        }

        private void UpdatePlaceholderVisibility(PasswordBox? passwordBox)
        {
            if (passwordBox != null)
            {
                var placeholder = passwordBox.Template.FindName("PlaceholderText", passwordBox) as TextBlock;
                if (placeholder != null)
                {
                    placeholder.Visibility = string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }

    public class LoginResponse
    {
        [JsonProperty("userID")]
        public string? UserID { get; set; }

        [JsonProperty("encodeKey")]
        public string? EncodeKey { get; set; }

        [JsonProperty("serverID")]
        public string? ServerID { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }
}