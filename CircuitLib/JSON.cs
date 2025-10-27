using ModularPanels.JsonLib;
using ModularPanels.SignalLib;
using ModularPanels.TrackLib;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace ModularPanels.CircuitLib
{
    internal struct SimpleCircuitJsonData
    {
        public StringKey<Circuit> ID { get; set; }
        public string Desc { get; set; }
        public bool Active { get; set; }
    }

    internal struct LogicOperatorJsonData
    {
        public string Op { get; set; }
        public StringKey<Circuit> Circuit { get; set; }
    }

    internal struct LogicCircuitJsonData
    {
        public StringKey<Circuit> ID { get; set; }
        public List<LogicOperatorJsonData> Condition { get; set; }
        public List<LogicOperatorJsonData> ConditionOn { get; set; }
        public List<LogicOperatorJsonData> ConditionOff { get; set; }
        public string Desc { get; set; }
    }

    internal struct RouteCircuitJsonData
    {
        public StringKey<Circuit> ID { get; set; }
        public string Desc { get; set; }
        public TrackRouteLoader Route { get; set; }
    }

    internal struct CircuitJsonData
    {
        public List<SimpleCircuitJsonData> SimpleCircuits { get; set; }
        public List<LogicCircuitJsonData> LogicCircuits { get; set; }
        public List<RouteCircuitJsonData> RouteCircuits { get; set; }
    }

    [JsonConverter(typeof(CircuitDataLoaderJsonConverter))]
    public class CircuitDataLoader
    {
        internal CircuitJsonData? Data { get; set; }

        public void Load(CircuitComponent comp)
        {
            if (Data == null)
                return;

            if (Data.Value.SimpleCircuits != null)
            {
                foreach (var sc in Data.Value.SimpleCircuits)
                {
                    comp.AddOrUpdateInputCircuit(sc.ID, sc.Desc, sc.Active);
                }
            }
            if (Data.Value.LogicCircuits != null)
            {
                foreach (var lc in Data.Value.LogicCircuits)
                {
                    LogicCircuit circuit = new(lc.ID.Key);
                    comp.AddCircuit(lc.ID, circuit);

                    if (!string.IsNullOrEmpty(lc.Desc))
                        circuit.Description = lc.Desc;
                }

                foreach (var lc in Data.Value.LogicCircuits)
                {
                    if (lc.ID.IsNull)
                        continue;

                    Circuit circuit = lc.ID.Object!;
                    if (circuit is LogicCircuit logicCircuit)
                    {
                        if (lc.Condition != null)
                        {
                            foreach (var condData in lc.Condition)
                            {
                                comp.RegisterKey(condData.Circuit);
                                if (condData.Circuit.IsNull)
                                    continue;

                                CircuitCondition? cond = comp.CreateCircuitOperator(condData.Circuit.Key, condData.Op);
                                if (cond == null)
                                    continue;

                                logicCircuit.AddCondition(cond);
                            }
                        }
                        if (lc.ConditionOn != null)
                        {
                            foreach (var condData in lc.ConditionOn)
                            {
                                comp.RegisterKey(condData.Circuit);
                                if (condData.Circuit.IsNull)
                                    continue;

                                CircuitCondition? cond = comp.CreateCircuitOperator(condData.Circuit.Key, condData.Op);
                                if (cond == null)
                                    continue;

                                logicCircuit.AddOnCondition(cond);
                            }
                        }
                        if (lc.ConditionOff != null)
                        {
                            foreach (var condData in lc.ConditionOff)
                            {
                                comp.RegisterKey(condData.Circuit);
                                if (condData.Circuit.IsNull)
                                    continue;

                                CircuitCondition? cond = comp.CreateCircuitOperator(condData.Circuit.Key, condData.Op);
                                if (cond == null)
                                    continue;

                                logicCircuit.AddOffCondition(cond);
                            }
                        }
                    }
                }
            }
            if (Data.Value.RouteCircuits != null)
            {
                if (comp.Parent is Module mod)
                {
                    foreach (var rc in Data.Value.RouteCircuits)
                    {
                        comp.RegisterKey(rc.ID);
                        TrackRoute? route = rc.Route.Load(mod.ObjectBank);
                        if (route == null)
                            continue;

                        RouteCircuit circuit = new(rc.ID.Key, route)
                        {
                            Description = rc.Desc ?? ""
                        };
                        comp.AddCircuit(circuit);
                    }
                }
            }
        }
    }

    internal class CircuitDataLoaderJsonConverter : JsonConverter<CircuitDataLoader>
    {
        public override CircuitDataLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            CircuitJsonData? data = JsonSerializer.Deserialize<CircuitJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, CircuitDataLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    internal class SignalCircuitJsonData
    {
        public required string SignalID { get; set; }
        public required string Indication { get; set; }
        public StringKey<Circuit>? InputCircuit { get; set; }
        public StringKey<Circuit>? OutputCircuit { get; set; }
        public bool? Fixed { get; set; }
        public string? DropIndication { get; set; }
        public bool? ResetLatch { get; set; }
    }

    [JsonConverter(typeof(SignalCircuitLoaderJsonConverter))]
    public class SignalCircuitLoader
    {
        internal SignalCircuitJsonData? Data { get; set; }

        public SignalCircuit? Load(SignalComponent signalComp, CircuitComponent circuitComp)
        {
            if (Data == null)
                return null;

            SignalHead? head = signalComp.GetSignalHead(Data.SignalID);
            if (head == null)
                return null;

            bool fixedIndication = false;
            if (Data.Fixed != null)
                fixedIndication = Data.Fixed.Value;

            SignalCircuit circuit = new(head, Data.Indication, fixedIndication);

            if (Data.InputCircuit != null)
            {
                circuitComp.RegisterKey(Data.InputCircuit);
                circuit.SetInput(Data.InputCircuit.Object);
            }

            if (Data.OutputCircuit != null)
            {
                if (circuitComp.RegisterOrCreateInputCircuit(Data.OutputCircuit, out InputCircuit? inputCircuit))
                    circuit.SetOutput(inputCircuit);
            }

            if (Data.DropIndication != null)
            {
                circuit.SetDropIndication(Data.DropIndication);
            }

            if (Data.ResetLatch != null)
            {
                circuit.SetResetLatch(Data.ResetLatch.Value);
            }

            return circuit;
        }
    }

    internal class SignalCircuitLoaderJsonConverter : JsonConverter<SignalCircuitLoader>
    {
        public override SignalCircuitLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            SignalCircuitJsonData? data = JsonSerializer.Deserialize<SignalCircuitJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, SignalCircuitLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    internal class PointsCircuitJsonData
    {
        public required StringKey<TrackPoints> PointsID { get; set; }
        public StringKey<Circuit>? CircuitOutPointsNormal { get; set; }
        public StringKey<Circuit>? CircuitOutPointsReversed { get; set; }
        public StringKey<Circuit>? CircuitInPointsNormal { get; set; }
        public StringKey<Circuit>? CircuitInPointsReverse { get; set; }
    }

    [JsonConverter(typeof(PointsCircuitLoaderJsonConverter))]
    public class PointsCircuitLoader
    {
        internal PointsCircuitJsonData? Data { get; set; }

        public PointsCircuit? Load(ObjectBank bank, CircuitComponent comp)
        {
            if (Data == null)
                return null;

            StringKey<TrackPoints> pointsKey = Data.PointsID;
            bank.RegisterKey(pointsKey);
            if (pointsKey.IsNull)
                return null;

            PointsCircuit? circuit = new(pointsKey.Object!);
            
            Circuit? cInNormal = null;
            if (Data.CircuitInPointsNormal != null)
            {
                comp.RegisterKey(Data.CircuitInPointsNormal);
                cInNormal = Data.CircuitInPointsNormal.Object;
            }
            Circuit? cInReverse = null;
            if (Data.CircuitInPointsReverse != null)
            {
                comp.RegisterKey(Data.CircuitInPointsReverse);
                cInReverse = Data.CircuitInPointsReverse.Object;
            }
            circuit.SetInputs(cInNormal, cInReverse);

            InputCircuit? cOutNormal = null;
            if (Data.CircuitOutPointsNormal != null)
            {
                comp.RegisterOrCreateInputCircuit(Data.CircuitOutPointsNormal, out cOutNormal);
            }
            InputCircuit? cOutReversed = null;
            if (Data.CircuitOutPointsReversed != null)
            {
                comp.RegisterOrCreateInputCircuit(Data.CircuitOutPointsReversed, out cOutReversed);
            }
            circuit.SetOutputs(cOutNormal, cOutReversed);

            return circuit;
        }
    }

    internal class PointsCircuitLoaderJsonConverter : JsonConverter<PointsCircuitLoader>
    {
        public override PointsCircuitLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            PointsCircuitJsonData? data = JsonSerializer.Deserialize<PointsCircuitJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, PointsCircuitLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    internal struct DetectorCircuitJsonData
    {
        public StringKey<TrackDetector> DetectorID { get; set; }
        public StringKey<Circuit> Circuit { get; set; }
    }

    [JsonConverter(typeof(DetectorCircuitLoaderJsonConverter))]
    public class DetectorCircuitLoader
    {
        internal DetectorCircuitJsonData? Data { get; set; }

        public DetectorCircuit? Load(ObjectBank bank, CircuitComponent comp)
        {
            if (Data == null)
                return null;

            bank.RegisterKey(Data.Value.DetectorID);

            if (Data.Value.DetectorID.IsNull)
                return null;

            if (comp.RegisterOrCreateInputCircuit(Data.Value.Circuit, out InputCircuit? inputCircuit))
            {
                DetectorCircuit circuit = new(Data.Value.DetectorID.Object!);
                circuit.SetOutput(inputCircuit);
            }

            return null;
        }
    }

    internal class DetectorCircuitLoaderJsonConverter : JsonConverter<DetectorCircuitLoader>
    {
        public override DetectorCircuitLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DetectorCircuitJsonData? data = JsonSerializer.Deserialize<DetectorCircuitJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, DetectorCircuitLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    internal struct BoundaryCircuitJsonData
    {
        public string Boundary { get; set; }
        public string ID { get; set; }
        public string? OutCircuit { get; set; }
        public string? InCircuit {  get; set; }
    }

    [JsonConverter(typeof(BoundaryCircuitJsonConverter))]
    public class BoundaryCircuitLoader
    {
        internal BoundaryCircuitJsonData? Data { get; set; }

        public BoundaryCircuit? Load(CircuitComponent comp)
        {
            if (Data == null)
                return null;

            if (!Enum.TryParse(Data.Value.Boundary, out BoundaryCircuit.BoundarySide side))
                return null;

            BoundaryCircuit circuit = new(Data.Value.ID, side);

            if (comp.TryGetCircuit<Circuit>(Data.Value.OutCircuit, out Circuit? outCircuit))
            {
                circuit.SetOutCircuit(outCircuit);
            }

            if (comp.TryGetCircuit(Data.Value.InCircuit, out InputCircuit? inCircuit))
            {
                circuit.SetInCircuit(inCircuit);
            }

            comp.AddBoundaryCircuit(circuit);

            return circuit;
        }
    }

    internal class BoundaryCircuitJsonConverter : JsonConverter<BoundaryCircuitLoader>
    {
        public override BoundaryCircuitLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            BoundaryCircuitJsonData? data = JsonSerializer.Deserialize<BoundaryCircuitJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, BoundaryCircuitLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
