using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModularPanels.DrawLib;

namespace ModularPanels.ButtonLib
{
    internal struct ControlLampJsonData
    {
        public int Size { get; set; }
        public float Border { get; set; }
        public ColorJS ColorOn { get; set; }
        public ColorJS ColorOff { get; set; }
    }

    internal struct RotarySwitchPositionJsonData
    {
        public float Angle { get; set; }
        public float Size { get; set; }
        public bool Latching { get; set; } = true;
        public ControlLampJsonData? Lamp { get; set; }
        public string? Text { get; set; }
        public string? TextStyle { get; set; }

        public RotarySwitchPositionJsonData() { }
    }

    internal struct RotarySwitchTemplateJsonData
    {
        public string Name { get; set; }
        public float Size { get; set; }
        public ColorJS PrimaryColor { get; set; }
        public ColorJS SecondaryColor { get; set; }
        public int CenterPos { get; set; }
        public List<RotarySwitchPositionJsonData> Positions { get; set; }
    }

    internal struct StateButtonStateJsonData
    {
        public ColorJS Color { get; set; }
    }

    internal struct StateButtonTemplateJsonData
    {
        public string Name { get; set; }
        public float Size { get; set; }
        public float RimSize { get; set; }
        public ColorJS PrimaryColor { get; set; }
        public int InitState { get; set; }
        public List<StateButtonStateJsonData> States { get; set; }
    }

    internal struct ControlTemplatesJsonData
    {
        public List<RotarySwitchTemplateJsonData>? RotarySwitches { get; set; }
        public List<StateButtonTemplateJsonData>? StateButtons { get; set; }
    }

    [JsonConverter(typeof(ControlTemplatesLoaderJsonConverter))]
    public class ControlTemplatesLoader
    {
        internal ControlTemplatesJsonData? Data { get; set; }

        public void Load()
        {
            if (Data == null)
                return;

            if (Data.Value.RotarySwitches != null)
            {
                foreach (var data in Data.Value.RotarySwitches)
                {
                    TemplateBank<RotarySwitchTemplate>.Instance.AddItem(new(data));
                }
            }

            if (Data.Value.StateButtons != null)
            {
                foreach (var data in Data.Value.StateButtons)
                {
                    TemplateBank<StateButtonTemplate>.Instance.AddItem(new(data));
                }
            }
        }

        public static void LoadTemplateFile(string path)
        {
            if (!File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            ControlTemplatesLoader? loader = JsonSerializer.Deserialize<ControlTemplatesLoader>(json);
            if (loader == null)
                return;

            loader.Load();
        }
    }

    internal class ControlTemplatesLoaderJsonConverter : JsonConverter<ControlTemplatesLoader>
    {
        public override ControlTemplatesLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ControlTemplatesJsonData? data = JsonSerializer.Deserialize<ControlTemplatesJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, ControlTemplatesLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
