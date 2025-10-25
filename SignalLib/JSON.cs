using ModularPanels.JsonLib;
using ModularPanels.TrackLib;
using System.Text.Json;
using System.Text.Json.Serialization;
using static ModularPanels.TrackLib.TrackRoute;

namespace ModularPanels.SignalLib
{
    internal struct DetectorLatchJsonData
    {
        public required StringKey<TrackDetector> ExitID {  get; set; }
        public required StringKey<TrackDetector> EntryID {  get; set; }
    }

    internal struct SignalRouteJsonData
    {
        public required SignalHeadId SigID { get; set; }
        public SignalHeadId? NextSigID { get; set; }
        public string Indication { get; set; }
        public TrackRouteLoader? Route { get; set; }
        public DetectorLatchJsonData? DetectorLatch { get; set; }
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
                nextSig = comp.GetRouteSignalHead(Data.Value.NextSigID.Value, true);

            TrackRoute? trackRoute = null;
            if (Data.Value.Route != null)
            {
                trackRoute = Data.Value.Route.Load(bank);
            }
                
            trackRoute ??= new();

            SignalRoute route = new(Data.Value.Indication, nextSig, trackRoute);

            if (Data.Value.DetectorLatch != null)
            {
                var detectorLatchData = Data.Value.DetectorLatch.Value;
                bank.RegisterKey(detectorLatchData.ExitID);
                bank.RegisterKey(detectorLatchData.EntryID);
                if (detectorLatchData.ExitID.TryGet(out TrackDetector? exitDetector) &&
                    detectorLatchData.EntryID.TryGet(out TrackDetector? entryDetector))
                {
                    DetectorLatch latch = new(exitDetector, entryDetector);
                    route.SetDetectorLatch(latch);
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

    internal struct BoundarySignalHeadJsonData
    {
        public string ID { get; set; }
        public string DefaultIndication { get; set; }
    }

    internal struct BoundarySignalJsonData
    {
        public string ID { get; set; }
        public string Type { get; set; }
        public string Boundary { get; set; }
        public int Index { get; set; }
        public List<BoundarySignalHeadJsonData> Heads { get; set; }
    }

    [JsonConverter(typeof(BoundarySignalLoaderJsonConverter))]
    public class BoundarySignalLoader
    {
        internal BoundarySignalJsonData? Data { get; set; }

        public void Load(Module module)
        {
            if (Data == null)
                return;

            if (!Enum.TryParse(Data.Value.Boundary, out BoundarySignal.BoundarySide boundary))
                boundary = BoundarySignal.BoundarySide.Left;

            SignalType? type = MainWindow.SignalBank[Data.Value.Type];
            if (type == null)
                return;

            BoundarySignal sig = new(module.GetSignalComponent(), Data.Value.ID, type, boundary, Data.Value.Index);
            foreach (var headData in Data.Value.Heads)
            {
                BoundarySignalHeadIn headIn = new(headData.ID, sig, headData.DefaultIndication);
                BoundarySignalHeadOut headOut = new(headData.ID, sig, headData.DefaultIndication);
                sig.AddHead(BoundarySignal.BoundaryDir.In, headIn);
                sig.AddHead(BoundarySignal.BoundaryDir.Out, headOut);
            }
            module.GetSignalComponent().AddBoundarySignal(sig);
        }
    }

    internal class BoundarySignalLoaderJsonConverter : JsonConverter<BoundarySignalLoader>
    {
        public override BoundarySignalLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            BoundarySignalJsonData? data = JsonSerializer.Deserialize<BoundarySignalJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, BoundarySignalLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
