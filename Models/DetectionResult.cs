// Models/DetectionResult.cs
using System.Collections.Generic;

namespace SphericalImageViewer.Models
{
    public class DetectionResult
    {
        public string ObjectName { get; set; }
        public double Confidence { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public List<Point2D> KeyPoints { get; set; }
        public string ModelUsed { get; set; }
        public double ProcessingTime { get; set; }
    }

    public class BoundingBox
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public double CenterX => X + Width / 2;
        public double CenterY => Y + Height / 2;
        public double Area => Width * Height;
    }

    public class Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Label { get; set; }
        public double Confidence { get; set; }
    }
}

