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

    internal struct JsonDataShapeOutline
    {
        public ColorJS Color { get; set; }
        public float Width { get; set; }
    }

    internal struct JsonDataShape
    {
        public string ID { set; get; }
        public int[][] Polygon { get; set; }
        public ColorJS Color { set; get; }
        public JsonDataShapeOutline? Outline { get; set; }
        public int[] Ellipse { get; set; }
        public int? Circle { get; set; }
        public int[] Rectangle { get; set; }
    }

    [JsonConverter(typeof(ShapeLoaderJsonConverter))]
    public class ShapeLoader
    {
        internal List<JsonDataShape>? Data { get; set; }

        public void Load(ShapeBank bank)
        {
            if (Data == null)
                return;

            foreach (var shapeData in Data)
            {
                Outline outline = new() { color = shapeData.Color };
                if (shapeData.Outline != null)
                {
                    outline.color = shapeData.Outline.Value.Color;
                    outline.width = shapeData.Outline.Value.Width;
                }

                Shape? shape = null;
                if (shapeData.Polygon != null)
                {
                    shape = new PolygonShape(shapeData.Polygon, shapeData.Color, outline);
                }
                else if (shapeData.Rectangle != null && shapeData.Rectangle.Length == 4)
                {
                    Point corner = new(shapeData.Rectangle[0], shapeData.Rectangle[1]);
                    Size size = new(shapeData.Rectangle[2], shapeData.Rectangle[3]);
                    shape = new RectangleShape(corner, size, shapeData.Color, outline);
                }
                else if (shapeData.Ellipse != null && shapeData.Ellipse.Length == 2)
                {
                    Size size = new(shapeData.Ellipse[0], shapeData.Ellipse[1]);
                    shape = new EllipseShape(size, shapeData.Color, outline);
                }
                else if (shapeData.Circle != null)
                {
                    int diameter = (int)shapeData.Circle;
                    Size size = new(diameter, diameter);
                    shape = new EllipseShape(size, shapeData.Color, outline);
                }

                if (shape != null)
                    bank.AddShape(shapeData.ID, shape);
            }
        }
    }

    public class ShapeLoaderJsonConverter : JsonConverter<ShapeLoader>
    {
        public override ShapeLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<JsonDataShape>? data = JsonSerializer.Deserialize<List<JsonDataShape>>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, ShapeLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    internal struct PanelRectJsonData
    {
        public GridPos TopLeft { get; set; }
        public GridPos BottomRight { get; set; }
        public ColorJS? FillColor { get; set; }
        public ColorJS BorderColor { get; set; }
        public float BorderSize { get; set; }
    }

    [JsonConverter(typeof(PanelRectJsonConverter))]
    public class PanelRectLoader
    {
        internal PanelRectJsonData? Data { get; set; }

        public PanelRect? Load()
        {
            if (Data == null)
                return null;

            Color? fillColor = null;
            if (Data.Value.FillColor != null)
                fillColor = Data.Value.FillColor;

            return new(fillColor, Data.Value.BorderColor, Data.Value.BorderSize, Data.Value.TopLeft, Data.Value.BottomRight);
        }
    }

    internal class PanelRectJsonConverter : JsonConverter<PanelRectLoader>
    {
        public override PanelRectLoader? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            PanelRectJsonData? data = JsonSerializer.Deserialize<PanelRectJsonData>(ref reader, options);
            return new() { Data = data };
        }

        public override void Write(Utf8JsonWriter writer, PanelRectLoader value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
