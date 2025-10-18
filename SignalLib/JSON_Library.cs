using ModularPanels.PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ModularPanels.SignalLib
{
    internal struct JsonDataSignalShape
    {
        public string Type { get; set; }
        public int[] Offset { get; set; }
        public float Angle { get; set; }
        public string[] Aspect { get; set; }
        public string Mirror { get; set; }
        public string? Head { get; set; }
    }

    internal struct JsonDataSignalHeadRuleset
    {
        public string Head { get; set; }
        public string Ruleset { get; set; }
    }

    internal struct JsonDataSignal
    {
        public string Name { get; set; }
        public string? Ruleset { get; set; }
        public List<JsonDataSignalHeadRuleset>? Rulesets { get; set; }
        public string? StartIndication { get; set; }
        public List<JsonDataSignalShape> Shapes { get; set; }
    }

    internal struct JsonDataSignalRule
    {
        public string Indication { get; set; }
        public string NextAspect { get; set; }
        public string Aspect { get; set; }
    }

    internal struct JsonDataSignalRuleset
    {
        public string ID { get; set; }
        public List<JsonDataSignalRule> Rules { get; set; }
        public List<string>? Aspects { get; set; }
        public string? DefaultIndication { get; set; }
    }

    internal class JsonDataSignalLibrary
    {
        public ShapeLoader? Shapes { get; set; }
        public List<JsonDataSignal>? Signals { get; set; }
        public List<JsonDataSignalRuleset>? Rulesets { get; set; }
    }

    [JsonConverter(typeof(JsonDataSignalLibraryJsonConverter))]
    public class SignalLibraryLoader
    {
        internal JsonDataSignalLibrary? Data { get; set; }

        public void Load(SignalBank bank)
        {
            if (Data == null)
                return;

            Data.Shapes?.Load(bank.ShapeBank);

            if (Data.Rulesets != null)
            {
                foreach (var rsData in Data.Rulesets)
                {
                    SignalRuleset ruleset = bank.CreateRuleset(rsData.ID);

                    if (rsData.DefaultIndication != null)
                    {
                        ruleset.DefaultIndication = rsData.DefaultIndication;
                    }

                    if (rsData.Aspects != null)
                    {
                        int i = 0;
                        foreach (string aspect in rsData.Aspects)
                        {
                            ruleset.SetAspectIndex(aspect, i);
                            i++;
                        }
                    }

                    foreach (var ruleData in rsData.Rules)
                    {
                        SignalRuleIndication indicationRules = ruleset.AddIndication(ruleData.Indication);
                        indicationRules.SetAspect(ruleData.NextAspect, ruleData.Aspect);
                    }
                }
            }

            if (Data.Signals != null)
            {
                foreach (var sig in Data.Signals)
                {
                    SignalRuleset? ruleset = null;
                    if (sig.Ruleset != null)
                        ruleset = bank.GetRuleset(sig.Ruleset);

                    SignalType type = new(sig.Name, ruleset);

                    if (sig.Shapes != null)
                    {
                        foreach (var shape in sig.Shapes)
                        {
                            ShapeMirror mirror = ShapeMirror.None;
                            if (shape.Mirror != null)
                            {
                                _ = Enum.TryParse(shape.Mirror, out mirror);
                            }
                            type.AddShape(shape.Type, shape.Aspect, shape.Offset, shape.Angle, shape.Head, mirror);
                        }
                    }

                    if (sig.StartIndication != null)
                        type.StartIndication = sig.StartIndication;
                    else if (ruleset != null)
                        type.StartIndication = ruleset.DefaultIndication;

                    if (sig.Rulesets != null)
                    {
                        foreach (var sigRs in sig.Rulesets)
                        {
                            SignalRuleset? headRuleset = bank.GetRuleset(sigRs.Ruleset);
                            if (headRuleset == null)
                                continue;

                            type.AddRuleset(sigRs.Head, headRuleset);
                        }
                    }

                    bank.AddType(type);
                }
            }
        }
    }

    internal class JsonDataSignalLibraryJsonConverter : JsonConverter<SignalLibraryLoader>
    {
        public override SignalLibraryLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataSignalLibrary? data = JsonSerializer.Deserialize<JsonDataSignalLibrary>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, SignalLibraryLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
