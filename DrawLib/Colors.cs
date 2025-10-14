using ModularPanels.JsonLib;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels.DrawLib
{
    public class CustomColor
    {
        readonly StringId<CustomColor> _id;
        readonly int _r, _g, _b;

        public string Name
        {
            get { return _id.Id; }
        }

        public CustomColor(StringId<CustomColor> id, int r, int g, int b)
        {
            _id = id;
            _r = r; _g = g; _b = b;
        }

        public static implicit operator Color(CustomColor color) => Color.FromArgb(color._r, color._g, color._b);
    }

    internal struct JsonDataCustomColor
    {
        public StringId<CustomColor> Name { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }

    [JsonConverter(typeof(CustomColorLoaderJsonConverter))]
    public class CustomColorLoader
    {
        internal JsonDataCustomColor? Data { get; set; }

        public CustomColor? Load(ObjectBank bank)
        {
            if (Data == null)
                return null;

            CustomColor color = new(Data.Value.Name, Data.Value.R, Data.Value.G, Data.Value.B);
            bank.DefineObject(color.Name, color);
            return color;
        }
    }

    public class CustomColorLoaderJsonConverter : JsonConverter<CustomColorLoader>
    {
        public override CustomColorLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataCustomColor? data = JsonSerializer.Deserialize<JsonDataCustomColor>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, CustomColorLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(ColorJsonConverter))]
    public class ColorJS
    {
        readonly string _name;

        public ColorJS(string name)
        {
            _name = name;
        }

        Color ToColor()
        {
            StringId<CustomColor> _customColorId = new(_name);
            GlobalBank.Instance.AssignId(ref _customColorId);

            if (_customColorId.IsNull)
                return Color.FromName(_name);

            return _customColorId.Get()!;
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
}
