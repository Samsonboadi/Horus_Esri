// Models/SegmentationResult.cs
using System.Collections.Generic;

namespace SphericalImageViewer.Models
{
    public class SegmentationResult
    {
        public List<Segment> Segments { get; set; } = new List<Segment>();
        public int SegmentCount => Segments.Count;
        public string ModelUsed { get; set; }
        public double ProcessingTime { get; set; }
        public string MaskImagePath { get; set; }
    }

    public class Segment
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public double Confidence { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public List<List<Point2D>> Contours { get; set; }
        public double Area { get; set; }
        public System.Drawing.Color Color { get; set; }
    }
}

