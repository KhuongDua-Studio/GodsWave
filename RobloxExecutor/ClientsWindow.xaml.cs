using QuorumAPI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RobloxExecutor
{
    public partial class ClientsWindow : Window
    {
        private DispatcherTimer updateTimer;

        public ClientsWindow()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;

            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            updateTimer.Tick += (s, e) => LoadRobloxClients();
            updateTimer.Start();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }

        private void LoadRobloxClients()
        {
            checkBoxContainer.Children.Clear();

            Process[] robloxProcesses = Process.GetProcessesByName("RobloxPlayerBeta");
            TitleActiveClients.Text = $"Hoạt Động Clients ({robloxProcesses.Length})";

            foreach (var process in robloxProcesses)
            {
                try
                {
                    TextBlock clientInfo = new TextBlock
                    {
                        Text = $"PID: {process.Id}",
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(10, 5, 10, 0),
                        FontSize = 14
                    };

                    checkBoxContainer.Children.Add(clientInfo);
                }
                catch
                {
                    continue;
                }
            }

            if (robloxProcesses.Length == 0)
            {
                TextBlock noClientText = new TextBlock
                {
                    Text = "Không có client Roblox đang chạy.",
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(10, 5, 10, 0),
                    FontSize = 14
                };
                checkBoxContainer.Children.Add(noClientText);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
