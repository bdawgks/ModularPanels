using ModularPanels.DrawLib;
using ModularPanels.JsonLib;
using ModularPanels.PanelLib;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    internal struct JsonDataGridStyle
    {
        public StringKey<GridStyle> ID { get; set; }
        public ColorJS MajorColor { get; set; }
        public ColorJS MinorColor { get; set; }
        public ColorJS TextColor { get; set; }
    }

    [JsonConverter(typeof(TrackStyleLoaderJsonConverter))]
    public class TrackStyleLoader()
    {
        internal JsonDataTrackStyle? Data { get; set; }

        public TrackStyle? Load(ObjectBank bank)
        {
            if (Data == null)
                return null;

            TrackStyle style = new()
            {
                color = Data.Value.Color,
                width = Data.Value.Width,
            };
            bank.DefineObject(Data.Value.ID, style);

            return style;
        }
    }

    internal class TrackStyleLoaderJsonConverter : JsonConverter<TrackStyleLoader>
    {
        public override TrackStyleLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataTrackStyle? data = JsonSerializer.Deserialize<JsonDataTrackStyle>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, TrackStyleLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(PointsStyleLoaderJsonConverter))]
    public class PointsStyleLoader()
    {
        internal JsonDataPointsStyle? Data { get; set; }

        public PointsStyle? Load(ObjectBank bank)
        {
            if (Data == null)
                return null;

            PointsStyle style = new()
            {
                colorInactive = Data.Value.ColorInactive,
                colorLock = Data.Value.ColorLock,
                length = Data.Value.Length,
                lockLength = Data.Value.LockLength,
                lockSpace = Data.Value.LockSpace,
                lockWidth = Data.Value.LockWidth
            };
            bank.DefineObject(Data.Value.ID, style);

            return style;
        }
    }

    internal class PointsStyleLoaderJsonConverter : JsonConverter<PointsStyleLoader>
    {
        public override PointsStyleLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataPointsStyle? data = JsonSerializer.Deserialize<JsonDataPointsStyle>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, PointsStyleLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(DetectorStyleLoaderJsonConverter))]
    public class DetectorStyleLoader()
    {
        internal JsonDataDetectorStyle? Data { get; set; }

        public DetectorStyle? Load(ObjectBank bank)
        {
            if (Data == null)
                return null;

            DetectorStyle? style = null;
            switch (Data.Value.Style)
            {
                case "Rectangle":
                    DetectorStyleRectangle rectStyle = new();
                    if (Data.Value.Rectangle != null)
                    {
                        rectStyle.minEdgeMargin = Data.Value.Rectangle.Value.MinEdgeMargin;
                        rectStyle.segmentSpace = Data.Value.Rectangle.Value.SegmentSpace;
                        rectStyle.segmentLength = Data.Value.Rectangle.Value.SegmentLength;
                        rectStyle.width = Data.Value.Rectangle.Value.Width;
                    }
                    style = rectStyle;
                    break;
            }

            style ??= DetectorStyle.Default;

            style.outline = Data.Value.OutlineSize;
            style.colorOutline = Data.Value.ColorOutline;
            style.colorEmpty = Data.Value.ColorEmpty;
            style.colorOccupied = Data.Value.ColorOccupied;

            bank.DefineObject(Data.Value.ID, style);

            return style;
        }
    }

    internal class DetectorStyleLoaderJsonConverter : JsonConverter<DetectorStyleLoader>
    {
        public override DetectorStyleLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataDetectorStyle? data = JsonSerializer.Deserialize<JsonDataDetectorStyle>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, DetectorStyleLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(GridStyleLoaderJsonConverter))]
    public class GridStyleLoader
    {
        internal JsonDataGridStyle? Data { get; set; }

        public GridStyle? Load(ObjectBank bank)
        {
            if (Data == null)
                return null;

            GridStyle style = new()
            {
                majorColor = Data.Value.MajorColor,
                minorColor = Data.Value.MinorColor,
                textColor = Data.Value.TextColor
            };
            bank.DefineObject(Data.Value.ID, style);

            return style;
        }
    }

    internal class GridStyleLoaderJsonConverter : JsonConverter<GridStyleLoader>
    {
        public override GridStyleLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataGridStyle? data = JsonSerializer.Deserialize<JsonDataGridStyle>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, GridStyleLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    internal struct RoutePointsJsonData
    {
        public required StringKey<TrackPoints> PointsID { get; set; }
        public required TrackPoints.PointsState Direction { get; set; }
    }

    internal class PointsStateJsonConverter : JsonConverter<TrackPoints.PointsState>
    {
        public override TrackPoints.PointsState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? str = reader.GetString();
            if (string.IsNullOrEmpty(str))
                return TrackPoints.PointsState.Normal;

            if (!Enum.TryParse(str, out TrackPoints.PointsState state))
                return TrackPoints.PointsState.Normal;

            return state;
        }

        public override void Write(Utf8JsonWriter writer, TrackPoints.PointsState value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(TrackRouteJsonConverter))]
    public class TrackRouteLoader
    {
        internal List<RoutePointsJsonData>? Data { get; set; }

        public TrackRoute? Load(ObjectBank bank)
        {
            if (Data == null)
                return null;

            TrackRoute route = new();
            
            foreach (var prData in Data)
            {
                bank.RegisterKey(prData.PointsID);
                if (!prData.PointsID.TryGet(out TrackPoints? points))
                    continue;

                route.AddPoints(points, prData.Direction);
            }

            return route;
        }
    }

    internal class TrackRouteJsonConverter : JsonConverter<TrackRouteLoader>
    {
        public override TrackRouteLoader Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<RoutePointsJsonData>? data = JsonSerializer.Deserialize<List<RoutePointsJsonData>>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, TrackRouteLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
