// Converters/BooleanToVisibilityConverter.cs
using SphericalImageViewer.Models;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SphericalImageViewer.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                var invert = parameter?.ToString()?.ToLower() == "inverse";

                if (invert)
                    boolValue = !boolValue;

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                var invert = parameter?.ToString()?.ToLower() == "inverse";
                var result = visibility == Visibility.Visible;

                return invert ? !result : result;
            }

            return false;
        }
    }
}

// Converters/BoolToColorConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SphericalImageViewer.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected
                    ? new SolidColorBrush(Colors.Green)
                    : new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

// Converters/BoolToStatusConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace SphericalImageViewer.Converters
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected ? "Connected" : "Disconnected";
            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}



// UI/MainDockPaneView.xaml.cs
using System.Windows.Controls;

namespace SphericalImageViewer.UI
{
    /// <summary>
    /// Interaction logic for MainDockPaneView.xaml
    /// </summary>
    public partial class MainDockPaneView : UserControl
    {
        public MainDockPaneView()
        {
            InitializeComponent();
        }
    }
}

// UI/SettingsDialog.xaml
using System.Windows;
using SphericalImageViewer.Models;

namespace SphericalImageViewer.UI
{
    public partial class SettingsDialog : Window
    {
        private UserSettings _settings;

        public SettingsDialog(UserSettings settings)
        {
            InitializeComponent();
            _settings = settings.Clone(); // Assuming we implement ICloneable
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

