using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SphericalImageViewer.Models;
using SphericalImageViewer.Services;

namespace SphericalImageViewer.ViewModels
{
    internal class MainDockPaneViewModel : DockPane
    {
        private const string _dockPaneID = "SphericalImageViewer_MainDockPane";

        #region Private Fields
        private readonly PythonApiService _apiService;
        private readonly SettingsService _settingsService;
        private ImageFrame _currentFrame;
        private List<ImageFrame> _imageFrames;
        private int _currentFrameIndex = 0;
        #endregion

        #region Properties

        private string _serverUrl = "http://192.168.6.100:5050";
        public string ServerUrl
        {
            get => _serverUrl;
            set => SetProperty(ref _serverUrl, value);
        }

        private string _imageDirectory = "/web/images/";
        public string ImageDirectory
        {
            get => _imageDirectory;
            set => SetProperty(ref _imageDirectory, value);
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
            set
            {
                if (SetProperty(ref _yaw, value))
                {
                    UpdateImageView();
                }
            }
        }

        private double _pitch = -20.0;
        public double Pitch
        {
            get => _pitch;
            set
            {
                if (SetProperty(ref _pitch, value))
                {
                    UpdateImageView();
                }
            }
        }

        private double _roll = 0.0;
        public double Roll
        {
            get => _roll;
            set
            {
                if (SetProperty(ref _roll, value))
                {
                    UpdateImageView();
                }
            }
        }

        private double _fov = 110.0;
        public double Fov
        {
            get => _fov;
            set
            {
                if (SetProperty(ref _fov, value))
                {
                    UpdateImageView();
                }
            }
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

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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

        private string _frameInfo = "Frame: 0/0";
        public string FrameInfo
        {
            get => _frameInfo;
            set => SetProperty(ref _frameInfo, value);
        }

        private bool _canNavigateFrames = false;
        public bool CanNavigateFrames
        {
            get => _canNavigateFrames;
            set => SetProperty(ref _canNavigateFrames, value);
        }

        private ObservableCollection<DetectionResult> _detectionResults = new ObservableCollection<DetectionResult>();
        public ObservableCollection<DetectionResult> DetectionResults
        {
            get => _detectionResults;
            set => SetProperty(ref _detectionResults, value);
        }

        #endregion

        #region Commands

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand LoadImagesCommand { get; }
        public ICommand RunDetectionCommand { get; }
        public ICommand RunSegmentationCommand { get; }
        public ICommand ResetViewCommand { get; }
        public ICommand PreviousFrameCommand { get; }
        public ICommand NextFrameCommand { get; }
        public ICommand FirstFrameCommand { get; }
        public ICommand LastFrameCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ExportResultsCommand { get; }

        #endregion

        public MainDockPaneViewModel()
        {
            _apiService = new PythonApiService();
            _settingsService = new SettingsService();
            _imageFrames = new List<ImageFrame>();

            LoadSettings();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            ConnectCommand = new AsyncRelayCommand(ConnectToServerAsync);
            DisconnectCommand = new RelayCommand(DisconnectFromServer);
            LoadImagesCommand = new AsyncRelayCommand(LoadImagesAsync);
            RunDetectionCommand = new AsyncRelayCommand(RunDetectionAsync);
            RunSegmentationCommand = new AsyncRelayCommand(RunSegmentationAsync);
            ResetViewCommand = new RelayCommand(ResetView);
            PreviousFrameCommand = new RelayCommand(PreviousFrame, () => _currentFrameIndex > 0);
            NextFrameCommand = new RelayCommand(NextFrame, () => _currentFrameIndex < _imageFrames.Count - 1);
            FirstFrameCommand = new RelayCommand(FirstFrame, () => _imageFrames.Count > 0 && _currentFrameIndex > 0);
            LastFrameCommand = new RelayCommand(LastFrame, () => _imageFrames.Count > 0 && _currentFrameIndex < _imageFrames.Count - 1);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ExportResultsCommand = new AsyncRelayCommand(ExportResultsAsync);
        }

        #region Command Methods

        private async Task ConnectToServerAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Connecting to Horus server...";

                var response = await _apiService.GetAsync<ServerInfo>("health");
                if (response.Success)
                {
                    IsConnected = true;
                    StatusMessage = $"Connected to Horus server successfully - {response.Data.Version}";
                    await LoadAvailableDirectoriesAsync();
                }
                else
                {
                    StatusMessage = $"Connection failed: {response.Error}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void DisconnectFromServer()
        {
            IsConnected = false;
            StatusMessage = "Disconnected from server";
            _imageFrames.Clear();
            CurrentImage = null;
            CanNavigateFrames = false;
            FrameInfo = "Frame: 0/0";
        }

        private async Task LoadImagesAsync()
        {
            if (!IsConnected) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Loading image frames...";

                var request = new LoadImagesRequest
                {
                    Directory = ImageDirectory,
                    ServerUrl = ServerUrl
                };

                var response = await _apiService.PostAsync<List<ImageFrame>>("images/load", request);
                if (response.Success && response.Data != null)
                {
                    _imageFrames = response.Data;
                    _currentFrameIndex = 0;

                    if (_imageFrames.Count > 0)
                    {
                        await LoadCurrentFrame();
                        CanNavigateFrames = _imageFrames.Count > 1;
                        UpdateFrameInfo();
                        StatusMessage = $"Loaded {_imageFrames.Count} image frames";
                    }
                    else
                    {
                        StatusMessage = "No images found in specified directory";
                    }
                }
                else
                {
                    StatusMessage = $"Failed to load images: {response.Error}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading images: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RunDetectionAsync()
        {
            if (_currentFrame == null || string.IsNullOrWhiteSpace(DetectionText)) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Running object detection...";

                var request = new DetectionRequest
                {
                    ImagePath = _currentFrame.Path,
                    Model = SelectedModel,
                    DetectionText = DetectionText,
                    Yaw = Yaw,
                    Pitch = Pitch,
                    Roll = Roll,
                    Fov = Fov
                };

                var response = await _apiService.PostAsync<List<DetectionResult>>("detection/detect", request);
                if (response.Success && response.Data != null)
                {
                    DetectionResults.Clear();
                    foreach (var result in response.Data)
                    {
                        DetectionResults.Add(result);
                    }

                    StatusMessage = $"Detection completed - found {response.Data.Count} objects";
                    await UpdateImageWithDetections();
                }
                else
                {
                    StatusMessage = $"Detection failed: {response.Error}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Detection error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RunSegmentationAsync()
        {
            if (_currentFrame == null || string.IsNullOrWhiteSpace(DetectionText)) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Running segmentation...";

                var request = new SegmentationRequest
                {
                    ImagePath = _currentFrame.Path,
                    Model = SelectedModel,
                    DetectionText = DetectionText,
                    Yaw = Yaw,
                    Pitch = Pitch,
                    Roll = Roll,
                    Fov = Fov
                };

                var response = await _apiService.PostAsync<SegmentationResult>("segmentation/segment", request);
                if (response.Success && response.Data != null)
                {
                    StatusMessage = $"Segmentation completed - {response.Data.SegmentCount} segments found";
                    await UpdateImageWithSegmentation(response.Data);
                }
                else
                {
                    StatusMessage = $"Segmentation failed: {response.Error}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Segmentation error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetView()
        {
            Yaw = 0.0;
            Pitch = -20.0;
            Roll = 0.0;
            Fov = 110.0;
            StatusMessage = "View reset to defaults";
        }

        private async void PreviousFrame()
        {
            if (_currentFrameIndex > 0)
            {
                _currentFrameIndex--;
                await LoadCurrentFrame();
                UpdateFrameInfo();
            }
        }

        private async void NextFrame()
        {
            if (_currentFrameIndex < _imageFrames.Count - 1)
            {
                _currentFrameIndex++;
                await LoadCurrentFrame();
                UpdateFrameInfo();
            }
        }

        private async void FirstFrame()
        {
            _currentFrameIndex = 0;
            await LoadCurrentFrame();
            UpdateFrameInfo();
        }

        private async void LastFrame()
        {
            _currentFrameIndex = _imageFrames.Count - 1;
            await LoadCurrentFrame();
            UpdateFrameInfo();
        }

        private void OpenSettings()
        {
            var settingsDialog = new SettingsDialog(_settingsService.GetSettings());
            if (settingsDialog.ShowDialog() == true)
            {
                var settings = settingsDialog.GetSettings();
                _settingsService.SaveSettings(settings);
                LoadSettings();
            }
        }

        private async Task ExportResultsAsync()
        {
            try
            {
                if (DetectionResults.Count == 0)
                {
                    MessageBox.Show("No detection results to export.", "Export Results");
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv",
                    DefaultExt = "json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await ExportDetectionResults(saveDialog.FileName);
                    StatusMessage = $"Results exported to {Path.GetFileName(saveDialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Export error: {ex.Message}";
            }
        }

        #endregion

        #region Helper Methods

        private async Task LoadCurrentFrame()
        {
            if (_currentFrameIndex < 0 || _currentFrameIndex >= _imageFrames.Count) return;

            _currentFrame = _imageFrames[_currentFrameIndex];
            await UpdateImageView();
        }

        private async Task UpdateImageView()
        {
            if (_currentFrame == null) return;

            try
            {
                var request = new RenderRequest
                {
                    ImagePath = _currentFrame.Path,
                    Yaw = Yaw,
                    Pitch = Pitch,
                    Roll = Roll,
                    Fov = Fov,
                    Width = 800,
                    Height = 600
                };

                var response = await _apiService.PostAsync<byte[]>("images/render", request);
                if (response.Success && response.Data != null)
                {
                    CurrentImage = CreateBitmapFromBytes(response.Data);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating image: {ex.Message}";
            }
        }

        private BitmapSource CreateBitmapFromBytes(byte[] imageBytes)
        {
            using (var stream = new MemoryStream(imageBytes))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        private void UpdateFrameInfo()
        {
            FrameInfo = $"Frame: {_currentFrameIndex + 1}/{_imageFrames.Count}";
        }

        private async Task LoadAvailableDirectoriesAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<List<string>>("directories");
                if (response.Success && response.Data != null)
                {
                    // Could populate a dropdown with available directories
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading directories: {ex.Message}");
            }
        }

        private async Task UpdateImageWithDetections()
        {
            if (DetectionResults.Count == 0) return;

            try
            {
                var request = new RenderWithDetectionsRequest
                {
                    ImagePath = _currentFrame.Path,
                    Yaw = Yaw,
                    Pitch = Pitch,
                    Roll = Roll,
                    Fov = Fov,
                    Width = 800,
                    Height = 600,
                    Detections = DetectionResults.ToList()
                };

                var response = await _apiService.PostAsync<byte[]>("images/render-with-detections", request);
                if (response.Success && response.Data != null)
                {
                    CurrentImage = CreateBitmapFromBytes(response.Data);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating image with detections: {ex.Message}";
            }
        }

        private async Task UpdateImageWithSegmentation(SegmentationResult segmentationResult)
        {
            try
            {
                var request = new RenderWithSegmentationRequest
                {
                    ImagePath = _currentFrame.Path,
                    Yaw = Yaw,
                    Pitch = Pitch,
                    Roll = Roll,
                    Fov = Fov,
                    Width = 800,
                    Height = 600,
                    SegmentationResult = segmentationResult
                };

                var response = await _apiService.PostAsync<byte[]>("images/render-with-segmentation", request);
                if (response.Success && response.Data != null)
                {
                    CurrentImage = CreateBitmapFromBytes(response.Data);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating image with segmentation: {ex.Message}";
            }
        }

        private void LoadSettings()
        {
            var settings = _settingsService.GetSettings();
            ServerUrl = settings.ServerUrl;
            ImageDirectory = settings.DefaultImageDirectory;
            SelectedModel = settings.DefaultModel;
        }

        private async Task ExportDetectionResults(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".json")
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(DetectionResults, Newtonsoft.Json.Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
            }
            else if (extension == ".csv")
            {
                var csv = "Object,Confidence,X,Y,Width,Height\n";
                foreach (var result in DetectionResults)
                {
                    csv += $"{result.ObjectName},{result.Confidence},{result.BoundingBox.X},{result.BoundingBox.Y},{result.BoundingBox.Width},{result.BoundingBox.Height}\n";
                }
                await File.WriteAllTextAsync(filePath, csv);
            }
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
}