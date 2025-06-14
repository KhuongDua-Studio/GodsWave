using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuorumAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RobloxExecutor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timertick;
            timer.Start();
            UpdateChecker.CheckForUpdateAsync();
        }

        public class UpdateInfo
        {
            public string version { get; set; }
            public string download_url { get; set; }
        }

        public class UpdateChecker
        {
            private static readonly string currentVersion = "1.2"; // Phiên bản hiện tại
            private static readonly string updateJsonUrl = "https://raw.githubusercontent.com/KhuongDua-Studio/DauHuYummyXP/refs/heads/main/hethong/kiem-tra-cap-nhat.json";

            public static async Task CheckForUpdateAsync()
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string json = await client.GetStringAsync(updateJsonUrl);
                        UpdateInfo update = JsonConvert.DeserializeObject<UpdateInfo>(json);

                        if (IsNewerVersion(update.version, currentVersion))
                        {
                            var result = MessageBox.Show($"Có bản cập nhật mới {update.version}. Bạn có muốn tải về không?",
                                "Cập nhật", MessageBoxButton.YesNo, MessageBoxImage.Information);

                            if (result == MessageBoxResult.Yes)
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = update.download_url,
                                    UseShellExecute = true
                                });
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Bạn đang sử dụng phiên bản mới nhất.", "Cập nhật", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể kiểm tra cập nhật: " + ex.Message);
                }
            }

            private static bool IsNewerVersion(string newVersion, string current)
            {
                Version newVer = new Version(newVersion);
                Version curVer = new Version(current);
                return newVer.CompareTo(curVer) > 0;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            webView21.NavigationCompleted += WebView21_NavigationCompleted;
            string indexPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"bin\monaco\index.html");
            webView21.Source = new Uri(indexPath);
        }

        private async void WebView21_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (webView21?.CoreWebView2 != null)
            {
                await LoadWebView21ContentAsync();
            }
        }

        private async Task SaveWebView21ContentAsync()
        {
            try
            {
                if (webView21?.CoreWebView2 != null)
                {
                    string jsonResult = await webView21.CoreWebView2.ExecuteScriptAsync("window.GetText();");
                    string script = JToken.Parse(jsonResult).ToString();

                    string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Tabs");
                    Directory.CreateDirectory(folderPath);

                    string filePath = System.IO.Path.Combine(folderPath, "main.lua");
                    File.WriteAllText(filePath, script);
                }
                else
                {
                    MessageBox.Show("⚠️ WebView21 chưa sẵn sàng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi lưu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadWebView21ContentAsync()
        {
            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Tabs", "main.lua");

                if (File.Exists(filePath))
                {
                    string content = await Task.Run(() => File.ReadAllText(filePath));
                    SetEditorText(content);
                }
                else
                {
                  //  MessageBox.Show("⚠️ Không tìm thấy file", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi tải: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private async void ExitTab_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đóng tab này không?",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                await SaveWebView21ContentAsync();

                try
                {
                    foreach (var proc in Process.GetProcessesByName("Đậu Hủ Yummy"))
                    {
                        proc.Kill();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể kill tiến trình: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Close();
            }
        }

        private void MiniTab_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        private void Trash_Click(object sender, RoutedEventArgs e)
        {
            SetEditorText("");
        }

        private async Task<string> GetEditorText()
        {
            if (webView21 != null)
            {
                string script = "GetText();";
                var result = await webView21.CoreWebView2.ExecuteScriptAsync(script);
                return result.Trim('"');
            }
            return string.Empty;
        }


        private void SetEditorText(string text)
        {
            if (webView21 != null)
            {
                string newText = text.Replace("\\", "\\\\").Replace("`", "\\`").Replace("\n", "\\n").Replace("\r", "\\r");
                webView21.CoreWebView2.ExecuteScriptAsync($"window.SetText(`{newText}`);");
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Tệp văn bản (*.txt;*.lua)|*.txt;*.lua|Tất cả tệp (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string textToSave = await GetEditorText();
                File.WriteAllText(saveFileDialog.FileName, textToSave);
            }
        }

        private void timertick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                bool isRobloxRunning = Process.GetProcessesByName("RobloxPlayerBeta").Length > 0;
                bool isInjected = CamTuAPI.IsRobloxOpen();

                Status.Foreground = new SolidColorBrush(isRobloxRunning && isInjected ? Colors.Green : Colors.Red);
            });
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Tệp văn bản (*.txt;*.lua)|*.txt;*.lua|Tất cả tệp (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string jsonResult = File.ReadAllText(openFileDialog.FileName);
                string text = jsonResult.Trim('"').Replace("\\\"", "\"").Replace("\\\\", "\\");

                if (!string.IsNullOrEmpty(text))
                {
                    SetEditorText(text);
                }
            }
        }

        private DispatcherTimer reinjectTimer = new DispatcherTimer();
        private bool wasInjected = false;

        private void Attach_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRobloxRunning())
            {
                MessageBox.Show("⚠️ Roblox chưa được bật. Hãy mở Roblox trước!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CamTuAPI.IsAutoInjectEnabled())
            {
                MessageBox.Show("✅ Đã inject!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                wasInjected = true;
                return;
            }

            InjectScript();
            wasInjected = true;

            reinjectTimer.Interval = TimeSpan.FromSeconds(3);
            reinjectTimer.Tick -= ReinjectTimer_Tick; // Tránh đăng ký nhiều lần
            reinjectTimer.Tick += ReinjectTimer_Tick;
            reinjectTimer.Start();
        }

        private void ReinjectTimer_Tick(object sender, EventArgs e)
        {
            if (!IsRobloxRunning())
            {
                wasInjected = false;
                return;
            }

            if (!CamTuAPI.IsAutoInjectEnabled() && !wasInjected)
            {
                InjectScript();
                wasInjected = true;
            }
        }

        private void InjectScript()
        {
            CamTuAPI.AttachAPI();
            CamTuAPI.ExecuteScript(@"
        if hookfunction and identifyexecutor then
            hookfunction(identifyexecutor,function() return 'Đậu Hủ Yummy','2.1' end)
        else
            identifyexecutor = function() return 'Đậu Hủ Yummy','2.1' end
        end

        local a = http_request or request or (syn and syn.request) or (fluxus and fluxus.request)
        if hookfunction and a then
            hookfunction(a, function(b)
                b.Headers = b.Headers or {}
                b.Headers['User-Agent'] = 'DauHuYummy/Roblox/NguyenMinhNhat/CPCamTu'
                return a(b)
            end)
        else
            warn('⚠️ Không thể hook http request')
        end
    ");
        }

        private bool IsRobloxRunning()
        {
            return Process.GetProcessesByName("RobloxPlayerBeta").Length > 0;
        }

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            string jsonResult = await webView21.CoreWebView2.ExecuteScriptAsync("window.GetText();");
            string script = JToken.Parse(jsonResult).ToString();
            CamTuAPI.ExecuteScript(script);
        }

        public static void ToggleWindow(Window window)
        {
            if (window == null)
                return;

            if (window.IsVisible)
            {
                window.Hide();
            }
            else
            {
                window.Show();
                window.Activate();
            }
        }

        private ClientsWindow _clientsWindow;
        private void Client_Click(object sender, RoutedEventArgs e)
        {
            if (_clientsWindow == null)
                _clientsWindow = new ClientsWindow();

            if (_clientsWindow.IsVisible)
                _clientsWindow.Hide();
            else
                _clientsWindow.Show();
        }

        private SettingsWindow _settingsWindow;
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow == null)
                _settingsWindow = new SettingsWindow();

            if (_settingsWindow.IsVisible)
                _settingsWindow.Hide();
            else
                _settingsWindow.Show();
        }
    }
}
