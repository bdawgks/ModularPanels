using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModularPanels.DrawLib
{

    [JsonConverter(typeof(ColorJsonConverter))]
    public readonly struct ColorJS(string colorStr)
    {
        readonly string _colorStr = colorStr;

        Color ToColor()
        {
            if (DrawLib.CustomColorBank.Instance.TryGetColor(_colorStr, out var customColor))
                return customColor;

            return Color.FromName(_colorStr);
        }

        public static implicit operator Color(ColorJS colorJS) => colorJS.ToColor();
    }

    public class ColorJsonConverter : JsonConverter<ColorJS>
    {
        public override ColorJS Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? colorString = reader.GetString();
            if (colorString != null)
            {
                return new(colorString);
            }

            return new("");
        }

        public override void Write(Utf8JsonWriter writer, ColorJS value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomColorBank : BankSingleton<Color>
    {
        public static CustomColorBank Instance
        {
            get => Instance<CustomColorBank>();
        }

        public bool HasColor(string name)
        {
            return HasItem(name);
        }

        public void AddColor(string name, Color color)
        {
            AddItem(name, color);
        }

        public bool TryGetColor(string name, out Color color)
        {
            return TryGetItem(name, out color);
        }
    }
}
