using QuorumAPI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RobloxExecutor
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            // Thêm sự kiện kéo cửa sổ
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }

        private void CheckBoxSettings_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = CheckBoxAutoAttach.IsChecked == true;
            CamTuAPI.SetAutoInject(isChecked);
        }

        private void CheckBoxTopMost_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = CheckBoxTopMost.IsChecked == true;
        }

        private void buttonResetTabs_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/@dauhuyummychannel",
                UseShellExecute = true
            });
        }

        private void buttonJoinDiscord_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/VM7ESrzccs",
                UseShellExecute = true
            });
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
