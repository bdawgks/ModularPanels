using ModularPanels.DrawLib;
using ModularPanels.JsonLib;

namespace ModularPanels.TrackLib
{
    internal struct JsonDataTrackNode
    {
        public StringKey<TrackNode> ID {  get; set; }
        public GridPos Pos {  get; set; }
        public bool? Square {  get; set; }
    }

    internal struct JsonDataTrackSegment
    {
        public StringKey<TrackSegment> ID {  get; set; }
        public List<StringKey<TrackNode>> Nodes { get; set; }
        public StringKey<TrackStyle> Style {  get; set; }
    }
    internal struct JsonDataTrackPoints
    {
        public StringKey<TrackPoints> ID { get; set; }
        public StringKey<TrackNode> PointsNode { get; set; }
        public StringKey<TrackNode> RouteNormal { get; set; }
        public StringKey<TrackNode> RouteReversed { get; set; }
        public StringKey<PointsStyle> Style { get; set; }
        public bool? UseBaseColor { get; set; }
    }

    internal struct JsonDataTrackDetector
    {
        public StringKey<TrackDetector> ID { get; set; }
        public StringKey<DetectorStyle> Style { get; set; }
        public List<StringKey<TrackSegment>> Segments { get; set; }
    }

    internal class JsonDataTrack
    {
        public List<JsonDataTrackNode>? Nodes { get; set; }
        public List<JsonDataTrackSegment>? Segments { get; set; }
        public List<JsonDataTrackPoints>? Points { get; set; }
        public List<JsonDataTrackDetector>? Detectors { get; set; }
    }

    internal struct JsonDataTrackStyle
    {
        public StringKey<TrackStyle> ID { get; set; }
        public ColorJS Color { get; set; }
        public float Width { get; set; }
    }

    internal struct JsonDataPointsStyle
    {
        public StringKey<PointsStyle> ID { get; set; }
        public ColorJS ColorInactive { get; set; }
        public ColorJS ColorLock { get; set; }
        public float LockLength { get; set; }
        public float LockWidth { get; set; }
        public float LockSpace { get; set; }
        public int Length { get; set; }
    }

    internal struct JsonDataDetectorStyleRectangle
    {
        public int MinEdgeMargin { get; set; }
        public int SegmentLength { get; set; }
        public int SegmentSpace { get; set; }
        public int Width { get; set; }
    }

    internal struct JsonDataDetectorStyle
    {
        public StringKey<DetectorStyle> ID { get; set; }
        public ColorJS ColorEmpty { get; set; }
        public ColorJS ColorOccupied { get; set; }
        public ColorJS ColorOutline { get; set; }
        public float OutlineSize { get; set; }
        public string Style { get; set; }
        public JsonDataDetectorStyleRectangle? Rectangle { get; set; }
    }
}
