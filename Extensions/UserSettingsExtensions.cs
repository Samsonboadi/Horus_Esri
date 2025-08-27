// Extensions/UserSettingsExtensions.cs
using SphericalImageViewer.Models;

namespace SphericalImageViewer.Extensions
{
    public static class UserSettingsExtensions
    {
        public static UserSettings Clone(this UserSettings settings)
        {
            return new UserSettings
            {
                ServerUrl = settings.ServerUrl,
                DefaultImageDirectory = settings.DefaultImageDirectory,
                DefaultModel = settings.DefaultModel,
                DefaultConfidenceThreshold = settings.DefaultConfidenceThreshold,
                DefaultIoUThreshold = settings.DefaultIoUThreshold,
                AutoSaveResults = settings.AutoSaveResults,
                ResultsDirectory = settings.ResultsDirectory,
                CameraDefaults = new CameraDefaults
                {
                    Yaw = settings.CameraDefaults.Yaw,
                    Pitch = settings.CameraDefaults.Pitch,
                    Roll = settings.CameraDefaults.Roll,
                    Fov = settings.CameraDefaults.Fov
                },
                UISettings = new UISettings
                {
                    ShowTooltips = settings.UISettings.ShowTooltips,
                    EnableAnimations = settings.UISettings.EnableAnimations,
                    Theme = settings.UISettings.Theme,
                    RememberWindowSize = settings.UISettings.RememberWindowSize,
                    AutoConnect = settings.UISettings.AutoConnect
                }
            };
        }
    }
}