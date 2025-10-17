using ModularPanels.JsonLib;
using ModularPanels.TrackLib;
using ModularPanels.SignalLib;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    internal struct CircuitJsonData
    {
        public List<SimpleCircuitJsonData> SimpleCircuits { get; set; }
        public List<LogicCircuitJsonData> LogicCircuits { get; set; }
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
                    SimpleCircuit circuit = new(sc.ID.Key);
                    if (sc.Active)
                        circuit.SetActive(true);

                    if (!string.IsNullOrEmpty(sc.Desc))
                        circuit.Description = sc.Desc;

                    comp.AddCircuit(circuit);
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
        }
    }

    public class CircuitDataLoaderJsonConverter : JsonConverter<CircuitDataLoader>
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
                circuitComp.RegisterKey(Data.OutputCircuit);
                if (Data.OutputCircuit.Object is SimpleCircuit)
                    circuit.SetOutput(Data.OutputCircuit.Object as SimpleCircuit);
            }

            return circuit;
        }
    }

    public class SignalCircuitLoaderJsonConverter : JsonConverter<SignalCircuitLoader>
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

            SimpleCircuit? cOutNormal = null;
            if (Data.CircuitOutPointsNormal != null)
            {
                comp.RegisterKey(Data.CircuitOutPointsNormal);
                cOutNormal = (SimpleCircuit?)Data.CircuitOutPointsNormal.Object;
            }
            SimpleCircuit? cOutReversed = null;
            if (Data.CircuitOutPointsReversed != null)
            {
                comp.RegisterKey(Data.CircuitOutPointsReversed);
                cOutReversed = (SimpleCircuit?)Data.CircuitOutPointsReversed.Object;
            }
            circuit.SetOutputs(cOutNormal, cOutReversed);

            return circuit;
        }
    }

    public class PointsCircuitLoaderJsonConverter : JsonConverter<PointsCircuitLoader>
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
}
