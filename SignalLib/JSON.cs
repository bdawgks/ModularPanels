using ModularPanels.JsonLib;
using ModularPanels.TrackLib;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels.SignalLib
{
    internal struct RoutePointsJsonData
    {
        public required StringKey<TrackPoints> PointsID { get; set; }
        public required string Direction { get; set; }
    }

    internal struct SignalRouteJsonData
    {
        public required SignalHeadId SigID { get; set; }
        public SignalHeadId? NextSigID { get; set; }
        public string Indication { get; set; }
        public List<RoutePointsJsonData>? Route { get; set; }
    }

    [JsonConverter(typeof(SignalRouteLoaderJsonConverter))]
    public class SignalRouteLoader
    {
        internal SignalRouteJsonData? Data { get; set; }

        public void Load(SignalComponent comp, ObjectBank bank)
        {
            if (Data == null)
                return;

            SignalHead? sig = comp.GetSignalHead(Data.Value.SigID);
            if (sig == null)
                return;

            SignalHead? nextSig = null;
            if (Data.Value.NextSigID != null)
                nextSig = comp.GetSignalHead(Data.Value.NextSigID.Value);
            SignalRoute route = new(Data.Value.Indication, nextSig);

            if (Data.Value.Route != null)
            {
                foreach (var rd in Data.Value.Route)
                {
                    PointsRoute pr = new();
                    bank.RegisterKey(rd.PointsID);
                    if (!rd.PointsID.TryGet(out TrackPoints? points))
                        continue;

                    pr.points = points;
                    if (!Enum.TryParse(rd.Direction, out TrackPoints.PointsState state))
                        continue;

                    pr.state = state;
                    route.AddPointsRoute(pr);
                }
            }
            sig.AddRoute(route);
        }
    }

    public class SignalRouteLoaderJsonConverter : JsonConverter<SignalRouteLoader>
    {
        public override SignalRouteLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            SignalRouteJsonData? data = JsonSerializer.Deserialize<SignalRouteJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, SignalRouteLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
