// Models/ServerInfo.cs
namespace SphericalImageViewer.Models
{
    public class ServerInfo
    {
        public string Version { get; set; }
        public string Status { get; set; }
        public ServerCapabilities Capabilities { get; set; }
        public ServerStats Stats { get; set; }
    }

    public class ServerCapabilities
    {
        public string[] SupportedModels { get; set; }
        public string[] SupportedImageFormats { get; set; }
        public string[] SupportedProjectionTypes { get; set; }
        public int MaxImageSize { get; set; }
        public int MaxBatchSize { get; set; }
    }

    public class ServerStats
    {
        public int ActiveConnections { get; set; }
        public int TotalRequestsProcessed { get; set; }
        public double AverageProcessingTime { get; set; }
        public string Uptime { get; set; }
        public double MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
    }
}

