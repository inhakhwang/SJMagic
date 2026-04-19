using System;
using System.Windows;
using System.Windows.Controls;
using SJMagic.ViewModels;

namespace SJMagic
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            this.DataContext = ViewModel;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Play();
            ViewModel.Log("Video Playing...", "AV");
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Pause();
            ViewModel.Log("Video Paused.", "AV");
        }

        private void txtLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLog.ScrollToEnd();
        }

        private void txtLog_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel.OpenLogFileCommand.CanExecute(null))
            {
                ViewModel.OpenLogFileCommand.Execute(null);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
