using ModularPanels.CircuitLib;
using ModularPanels.JsonLib;
using ModularPanels.SignalLib;
using ModularPanels.TrackLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModularPanels.BlockController
{
    internal struct PointsRouteJsonData
    {
        public StringKey<TrackPoints> PointsID { get; set; }
        public TrackPoints.PointsState Direction { get; set; }
    }

    internal struct RouteJsonData
    {
        public string ID {  get; set; }
        public List<PointsRouteJsonData> Route { get; set; }
    }

    internal struct BlockJsonData
    {
        public string ID { get; set; }
        public StringKey<TrackDetector>? Detector { get; set; }
    }

    internal struct SignalSetJsonData
    {
        public SignalHeadId SigID { get; set; }
        public string? Route { get; set; }
        public List<string> Blocks { get; set; }
        public List<SignalHeadIdOutOnly>? SetWith { get; set; }
        public string IndicationClear { get; set; }
        public string IndicationOccupied { get; set; }
        public string IndicationUnset { get; set; }
        public StringKey<Circuit> CircuitSet { get; set; }
        public StringKey<Circuit> CircuitUnset { get; set; }
        public StringKey<Circuit> CircuitLocked { get; set; }
        public bool? AutoUnset { get; set; }
    }

    internal struct BlockControllerJsonData
    {
        public List<RouteJsonData> Routes { get; set; }
        public List<BlockJsonData> Blocks { get; set; }
        public List<SignalSetJsonData> Signals { get; set; }
    }

    [JsonConverter(typeof(BlockControllerJsonConverter))]
    public class BlockControllerLoader
    {
        internal BlockControllerJsonData? Data { get; set; }

        public BlockController? Load(Module mod)
        {
            if (Data == null)
                return null;

            BlockController controller = new(mod);

            foreach (var rd in Data.Value.Routes)
            {
                TrackRoute route = new();
                
                foreach (var pd in rd.Route)
                {
                    mod.ObjectBank.RegisterKey(pd.PointsID);
                    if (!pd.PointsID.TryGet(out TrackPoints? points))
                        continue;

                    route.AddPoints(points, pd.Direction);
                }

                controller.AddRoute(rd.ID, route);
            }

            foreach (var bd in Data.Value.Blocks)
            {
                TrackDetector? detector = null;
                if (bd.Detector != null)
                {
                    mod.ObjectBank.RegisterKey(bd.Detector);
                    detector = bd.Detector.Object;
                }
                controller.AddBlock(bd.ID, detector);
            }

            foreach (var ss in Data.Value.Signals)
            {
                SignalHead? sig = mod.GetSignalComponent().GetSignalHead(ss.SigID);

                if (sig == null)
                    continue;

                List<SignalHead> setWith = [];
                if (ss.SetWith != null)
                {
                    foreach (var sws in ss.SetWith)
                    {
                        SignalHead? setWithSig = mod.GetSignalComponent().GetSignalHead(sws);
                        if (setWithSig != null)
                            setWith.Add(setWithSig);
                    }
                }

                BlockController.SignalSetParams pars = new()
                {
                    signal = sig,
                    route = ss.Route,
                    blocks = [.. ss.Blocks],
                    indicationClear = ss.IndicationClear,
                    indicationOccupied = ss.IndicationOccupied,
                    indicationUnset = ss.IndicationUnset,
                    autoUnset = ss.AutoUnset ?? false,
                    setWith = [.. setWith]
                };

                mod.GetCircuitComponent().RegisterKey(ss.CircuitSet);
                mod.GetCircuitComponent().RegisterKey(ss.CircuitUnset);

                InputCircuit? lockCircuit = null;
                if (ss.CircuitLocked != null)
                    mod.GetCircuitComponent().TryGetCircuit(ss.CircuitLocked.Key, out lockCircuit);
                controller.AddSignalSet(pars, ss.CircuitSet.Object, ss.CircuitUnset.Object, lockCircuit);
            }

            return controller;
        }
    }

    internal class BlockControllerJsonConverter : JsonConverter<BlockControllerLoader>
    {
        public override BlockControllerLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            BlockControllerJsonData? data = JsonSerializer.Deserialize<BlockControllerJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, BlockControllerLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
