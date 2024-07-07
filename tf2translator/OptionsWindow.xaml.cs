﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using tf2translator.Helpers;

namespace tf2translator
{
    public partial class OptionsWindow
    {
    
        public string CurrentVersion
        {
            get { return Version.CurrentVersion; }
        }
        
        public OptionsWindow()
        {
            InitializeComponent();
            LoadOptions();
            DataContext = this;
        }

        private void LoadOptions()
        {
            TbFolderPath.Text = OptionsManager.InstallationPath;
            TbLang.Text = OptionsManager.Language;
        }

        private void BtnSaveOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsManager.InstallationPath = TbFolderPath.Text;
            OptionsManager.Language = TbLang.Text;
            
            OptionsManager.Save();
            Close();
        }

        private void BtnSetDefault_Click(object sender, RoutedEventArgs e)
        {
            OptionsManager.SetDefault();
            LoadOptions();
        }
        
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
