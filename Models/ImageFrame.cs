// Models/ImageFrame.cs
using System;
using System.Collections.Generic;

namespace SphericalImageViewer.Models
{
    public class ImageFrame
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public DateTime Timestamp { get; set; }
        public int FrameNumber { get; set; }
        public string ThumbnailPath { get; set; }
        public ImageMetadata Metadata { get; set; }
    }

    public class ImageMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }
        public double FileSize { get; set; }
        public CameraParameters Camera { get; set; }
    }

    public class CameraParameters
    {
        public double DefaultYaw { get; set; }
        public double DefaultPitch { get; set; }
        public double DefaultRoll { get; set; }
        public double DefaultFov { get; set; }
        public string ProjectionType { get; set; }
    }
}

