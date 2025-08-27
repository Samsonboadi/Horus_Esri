using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace SphericalImageViewer.ViewModels
{
    internal class MainDockPaneViewModel : DockPane, INotifyPropertyChanged
    {
        private const string _dockPaneID = "SphericalImageViewer_MainDockPane";

        #region Properties

        private string _serverUrl = "http://192.168.6.100:5050/web/";
        public string ServerUrl
        {
            get => _serverUrl;
            set => SetProperty(ref _serverUrl, value);
        }

        private BitmapSource _currentImage;
        public BitmapSource CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        private double _yaw = 0.0;
        public double Yaw
        {
            get => _yaw;
            set => SetProperty(ref _yaw, value);
        }

        private double _pitch = -20.0;
        public double Pitch
        {
            get => _pitch;
            set => SetProperty(ref _pitch, value);
        }

        private double _roll = 0.0;
        public double Roll
        {
            get => _roll;
            set => SetProperty(ref _roll, value);
        }

        private double _fov = 110.0;
        public double Fov
        {
            get => _fov;
            set => SetProperty(ref _fov, value);
        }

        private string _detectionText = "pole";
        public string DetectionText
        {
            get => _detectionText;
            set => SetProperty(ref _detectionText, value);
        }

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<string> AvailableModels { get; } = new ObservableCollection<string>
        {
            "GroundingLangSAM",
            "GroundingDino",
            "YoloWorld",
            "SAM_V2",
            "Florence2"
        };

        private string _selectedModel = "GroundingLangSAM";
        public string SelectedModel
        {
            get => _selectedModel;
            set => SetProperty(ref _selectedModel, value);
        }

        #endregion

        #region Commands

        private ICommand _connectCommand;
        public ICommand ConnectCommand => _connectCommand ??= new RelayCommand(ConnectToServer);

        private ICommand _runDetectionCommand;
        public ICommand RunDetectionCommand => _runDetectionCommand ??= new RelayCommand(RunDetection);

        #endregion

        protected MainDockPaneViewModel() { }

        #region Methods

        private void ConnectToServer()
        {
            try
            {
                StatusMessage = "Connecting to server...";
                // TODO: Implement server connection
                StatusMessage = "Connected successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection failed: {ex.Message}";
            }
        }

        private void RunDetection()
        {
            try
            {
                StatusMessage = "Running detection...";
                // TODO: Implement detection
                StatusMessage = "Detection completed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Detection failed: {ex.Message}";
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class MainDockPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            MainDockPaneViewModel.Show();
        }
    }

    // Simple RelayCommand implementation if you don't have CommunityToolkit.Mvvm
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();
    }
}