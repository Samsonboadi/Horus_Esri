// Models/Settings.cs
namespace SphericalImageViewer.Models
{
    public class UserSettings
    {
        public string ServerUrl { get; set; } = "http://192.168.6.100:5050";
        public string DefaultImageDirectory { get; set; } = "/web/images/";
        public string DefaultModel { get; set; } = "GroundingLangSAM";
        public double DefaultConfidenceThreshold { get; set; } = 0.3;
        public double DefaultIoUThreshold { get; set; } = 0.5;
        public CameraDefaults CameraDefaults { get; set; } = new CameraDefaults();
        public UISettings UISettings { get; set; } = new UISettings();
        public bool AutoSaveResults { get; set; } = false;
        public string ResultsDirectory { get; set; } = @"C:\SphericalImageViewer\Results";
    }

    public class CameraDefaults
    {
        public double Yaw { get; set; } = 0.0;
        public double Pitch { get; set; } = -20.0;
        public double Roll { get; set; } = 0.0;
        public double Fov { get; set; } = 110.0;
    }

    public class UISettings
    {
        public bool ShowTooltips { get; set; } = true;
        public bool EnableAnimations { get; set; } = true;
        public string Theme { get; set; } = "Light";
        public bool RememberWindowSize { get; set; } = true;
        public bool AutoConnect { get; set; } = false;
    }
}