﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using tf2translator.Controllers;
using tf2translator.Enums;
using tf2translator.EventArgs;
using tf2translator.Exceptions;
using tf2translator.Helpers;
using tf2translator.Models;

namespace tf2translator
{
    public partial class MainWindow
    {
        private readonly DispatcherTimer _checkTimer = new DispatcherTimer();
        
        public static EventHandler<TranslatorExceptionEventArgs> ErrorEncountered;
        public static EventHandler<TranslatorExceptionEventArgs> Succeeded;

        public MainWindow()
        {
            InitializeComponent();
            CheckForUpdates();

            ErrorEncountered += OnErrorEncountered;
            Succeeded += OnSucceeded;

            LogsController.Logs = new LinkedList<Log>();
            LogsController.Chats = new List<Chat>();

            Loaded += LoadWindow;
        }

        private void OnErrorEncountered(object sender, TranslatorExceptionEventArgs args)
        {
           Dispatcher.Invoke(() => { LblError.Content = args.Exception.Message; }); 
        }
        
        private void OnSucceeded(object sender, TranslatorExceptionEventArgs e)
        { 
            Dispatcher.Invoke(() => {
                if ((string) LblError.Content == e.Exception.Message)
                    LblError.Content = "";
            });
        }

        private void LoadWindow(object sender, RoutedEventArgs e)
        {
            ChatView.ItemsSource = LogsController.Chats;

            _checkTimer.Interval = TimeSpan.FromSeconds(3);
            _checkTimer.Tick += TimerTick;
            _checkTimer.Start();

            OptionsManager.ValidateSettings();
        }

        private async void TimerTick(object sender, System.EventArgs e)
        {
            await LogsController.LoadLogsAsync(30);
            ChatView.Items.Refresh();
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();
            LogsController.Chats.Clear();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ChatView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        
        }

        private async void CheckForUpdates()
        {
            VersionChecker versionChecker = new VersionChecker();
            bool isNewVersionAvailable = await versionChecker.CheckForUpdates();

            if (isNewVersionAvailable)
            {
                // Display a notification to the user
                updateNotification.Text = "A new version is available. Click here to update.";
                updateNotification.Visibility = Visibility.Visible;
                dismissUpdate.Visibility = Visibility.Visible;
            }
        }

        private void UpdateNotification_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string downloadLink = "https://github.com/ParadoxLeon/TF2-Chat-translator/releases";
            try
            {
                Process.Start(new ProcessStartInfo(downloadLink) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening the download link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dismissUpdate_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            updateNotification.Visibility = Visibility.Collapsed;
            dismissUpdate.Visibility = Visibility.Collapsed;
        }
    }
}
