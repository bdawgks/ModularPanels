using ModularPanels.DrawLib;
using ModularPanels.JsonLib;

namespace ModularPanels.TrackLib
{
    internal struct JsonDataTrackNode
    {
        public StringId<TrackNode> ID {  get; set; }
        public GridPos Pos {  get; set; }
        public bool? Square {  get; set; }
    }

    internal struct JsonDataTrackSegment
    {
        public StringId<TrackSegment> ID {  get; set; }
        public List<StringId<TrackNode>> Nodes { get; set; }
        public StringId<TrackStyle> Style {  get; set; }
    }
    internal struct JsonDataTrackPoints
    {
        public StringId<TrackPoints> ID { get; set; }
        public StringId<TrackNode> PointsNode { get; set; }
        public StringId<TrackNode> RouteNormal { get; set; }
        public StringId<TrackNode> RouteReversed { get; set; }
        public StringId<PointsStyle> Style { get; set; }
        public bool? UseBaseColor { get; set; }
    }

    internal struct JsonDataTrackDetector
    {
        public StringId<TrackDetector> ID { get; set; }
        public StringId<DetectorStyle> Style { get; set; }
        public List<StringId<TrackSegment>> Segments { get; set; }
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
        public StringId<TrackStyle> ID { get; set; }
        public ColorJS Color { get; set; }
        public float Width { get; set; }
    }

    internal struct JsonDataPointsStyle
    {
        public StringId<PointsStyle> ID { get; set; }
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
        public StringId<DetectorStyle> ID { get; set; }
        public ColorJS ColorEmpty { get; set; }
        public ColorJS ColorOccupied { get; set; }
        public ColorJS ColorOutline { get; set; }
        public float OutlineSize { get; set; }
        public string Style { get; set; }
        public JsonDataDetectorStyleRectangle? Rectangle { get; set; }
    }
}
