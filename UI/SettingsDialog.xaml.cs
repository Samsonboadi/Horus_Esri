using System.Windows;
using SphericalImageViewer.Models;
using SphericalImageViewer.Extensions;

namespace SphericalImageViewer.UI
{
    public partial class SettingsDialog : Window
    {
        private UserSettings _settings;

        public SettingsDialog(UserSettings settings)
        {
            InitializeComponent();
            _settings = settings.Clone();
            DataContext = _settings;
        }

        public UserSettings GetSettings()
        {
            return _settings;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _settings = new UserSettings();
            DataContext = _settings;
        }
    }
}