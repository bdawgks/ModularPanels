using ModularPanels.JsonLib;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels.TrackLib
{
    [JsonConverter(typeof(TrackDataLoaderJsonConverter))]
    public class TrackDataLoader()
    {
        JsonDataTrack? data;

        internal JsonDataTrack? Data
        {
            get => data;
            set => data = value;
        }

        private static void ValidateTrackNode(ref StringKey<TrackNode> nodeId, ObjectBank bank)
        {
            bank.RegisterKey(nodeId);
            if (nodeId.IsNull)
                throw new Exception(string.Format("Invalid track node ID: {0}", nodeId.Key));
        }

        public void Load(ObjectBank bank)
        {
            if (data == null)
                return;

            if (data.Nodes != null)
            {
                foreach (var nodeData in data.Nodes)
                {
                    TrackNode node = new(nodeData.ID.Key, nodeData.Pos)
                    {
                        squareEnd = nodeData.Square != null && nodeData.Square.Value
                    };
                    bank.DefineObject(node.id, node);
                }
            }

            if (data.Segments != null)
            {
                foreach (var segData in data.Segments)
                {
                    if (segData.Nodes.Count != 2)
                        throw new Exception(string.Format("Invalid track segment node definition: requires 2 valid node IDs [{0}]", segData.ID.Key));

                    var n0 = segData.Nodes[0];
                    var n1 = segData.Nodes[1];

                    ValidateTrackNode(ref n0, bank);
                    ValidateTrackNode(ref n1, bank);

                    GlobalBank.Instance.RegisterKey(segData.Style);
                    TrackStyle style;
                    if (segData.Style.IsNull)
                        style = new();
                    else
                        style = segData.Style.Object!;

                    TrackSegment segment = new(segData.ID.Key, style, n0.Object!, n1.Object!);
                    bank.DefineObject(segment.id, segment);
                }
            }

            if (data.Points != null)
            {
                foreach (var pointsData in data.Points)
                {
                    var n0 = pointsData.PointsNode;
                    var n1 = pointsData.RouteNormal;
                    var n2 = pointsData.RouteReversed;

                    ValidateTrackNode(ref n0, bank);
                    ValidateTrackNode(ref n1, bank);
                    ValidateTrackNode(ref n2, bank);

                    GlobalBank.Instance.RegisterKey(pointsData.Style);
                    PointsStyle style;
                    if (pointsData.Style.IsNull)
                        style = new();
                    else
                        style = pointsData.Style.Object!;

                    bool useBaseColor = pointsData.UseBaseColor != null && pointsData.UseBaseColor.Value;
                    TrackPoints points = new(pointsData.ID.Key, n0.Object!, n1.Object!, n2.Object!, useBaseColor)
                    {
                        Style = style
                    };
                    bank.DefineObject(points.id, points);
                }
            }

            if (data.Detectors != null)
            {
                foreach (var detectorData in data.Detectors)
                {
                    GlobalBank.Instance.RegisterKey(detectorData.Style);
                    DetectorStyle style;
                    if (detectorData.Style.IsNull)
                        style = DetectorStyle.Default;
                    else
                        style = detectorData.Style.Object!;

                    TrackDetector detector = new(detectorData.ID.Key)
                    {
                        Style = style
                    };

                    foreach (StringKey<TrackSegment> segKey in detectorData.Segments)
                    {
                        bank.RegisterKey(segKey);
                        if (segKey.IsNull)
                            throw new Exception(string.Format("Invalid track segment ID: {0}", segKey.Key));

                        detector.AddSegment(segKey.Object!);
                    }

                    bank.DefineObject(detector.ID, detector);
                }
            }
        }
    }

    public class TrackDataLoaderJsonConverter : JsonConverter<TrackDataLoader>
    {
        public override TrackDataLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataTrack? data = JsonSerializer.Deserialize<JsonDataTrack>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, TrackDataLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
