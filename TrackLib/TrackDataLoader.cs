using ModularPanels.JsonLib;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

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

        private static void ValidateTrackNode(ref StringId<TrackNode> nodeId, ObjectBank bank)
        {
            bank.AssignId(ref nodeId);
            if (nodeId.IsNull)
                throw new Exception(string.Format("Invalid track node ID: {0}", nodeId.Id));
        }

        public void Load(ObjectBank bank)
        {
            if (data == null)
                return;

            if (data.Nodes != null)
            {
                foreach (var nodeData in data.Nodes)
                {
                    TrackNode node = new(nodeData.ID.Id, nodeData.Pos)
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
                        throw new Exception(string.Format("Invalid track segment node definition: requires 2 valid node IDs [{0}]", segData.ID.Id));

                    var n0 = segData.Nodes[0];
                    var n1 = segData.Nodes[1];

                    ValidateTrackNode(ref n0, bank);
                    ValidateTrackNode(ref n1, bank);

                    var styleId = segData.Style;
                    GlobalBank.Instance.AssignId(ref styleId);
                    TrackStyle style;
                    if (styleId.IsNull)
                        style = new();
                    else
                        style = styleId.Get()!;

                    TrackSegment segment = new(segData.ID.Id, style, n0.Get()!, n1.Get()!);
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

                    var styleId = pointsData.Style;
                    GlobalBank.Instance.AssignId(ref styleId);
                    PointsStyle style;
                    if (styleId.IsNull)
                        style = new();
                    else
                        style = styleId.Get()!;

                    bool useBaseColor = pointsData.UseBaseColor != null && pointsData.UseBaseColor.Value;
                    TrackPoints points = new(pointsData.ID.Id, n0.Get()!, n1.Get()!, n2.Get()!, useBaseColor)
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
                    var styleId = detectorData.Style;
                    GlobalBank.Instance.AssignId(ref styleId);
                    DetectorStyle style;
                    if (styleId.IsNull)
                        style = DetectorStyle.Default;
                    else
                        style = styleId.Get()!;

                    TrackDetector detector = new(detectorData.ID.Id)
                    {
                        Style = style
                    };

                    foreach (StringId<TrackSegment> s in detectorData.Segments)
                    {
                        var segId = s;
                        bank.AssignId(ref segId);
                        if (segId.IsNull)
                            throw new Exception(string.Format("Invalid track segment ID: {0}", segId.Id));

                        detector.AddSegment(segId.Get()!);
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
