using ModularPanels.DrawLib;
using ModularPanels.JsonLib;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels.PanelLib
{
    internal struct JsonDataTextStyle
    {
        public StringKey<TextStyle> ID { get; set; }
        public string Font { get; set; }
        public ColorJS Color { get; set; }
        public int Size { get; set; }
        public bool? Bold { get; set; }
    }

    [JsonConverter(typeof(TextStyleLoaderJsonConverter))]
    public class TextStyleLoader
    {
        internal JsonDataTextStyle? Data { get; set; }

        public TextStyle? Load(ObjectBank bank)
        {
            if (Data == null)
                return null;

            TextStyle style = new()
            {
                font = Data.Value.Font,
                color = Data.Value.Color,
                size = Data.Value.Size,
                bold = Data.Value.Bold != null && Data.Value.Bold.Value,
            };

            bank.DefineObject(Data.Value.ID, style);
            return style;
        }
    }

    public class TextStyleLoaderJsonConverter : JsonConverter<TextStyleLoader>
    {
        public override TextStyleLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataTextStyle? data = JsonSerializer.Deserialize<JsonDataTextStyle>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, TextStyleLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
