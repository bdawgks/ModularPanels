using PanelLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModularPanels
{
    public interface IJSONInitializer<T>
    {
        public T Initialize();
    }

    [JsonConverter(typeof(ColorJsonConverter))]
    public readonly struct ColorJS(string colorStr)
    {
        readonly string _colorStr = colorStr;

        Color ToColor()
        {
            if (PanelLib.CustomColorBank.Instance.TryGetColor(_colorStr, out var customColor))
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

    public struct JSON_CustomColor
    {
        public string Name { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public readonly Color GetColor()
        {
            return Color.FromArgb(R, G, B);
        }

        public readonly void AddToBank()
        {
            if (PanelLib.CustomColorBank.Instance.HasColor(Name))
                throw new Exception("Color with given name was already defined: " + Name);

            PanelLib.CustomColorBank.Instance.AddColor(Name, GetColor());
        }
    }

    public struct JSON_TrackStyle
    {
        public string ID { get; set; }
        public ColorJS Color { get; set; }
        public float Width { get; set; }

        public readonly PanelLib.TrackStyle Load()
        {
            PanelLib.TrackStyle style = new()
            {
                color = Color,
                width = Width
            };
            return style;
        }
    }

    public struct JSON_PointsStyle
    {
        public string ID { get; set; }
        public ColorJS ColorInactive { get; set; }
        public ColorJS ColorLock { get; set; }
        public float LockLength { get; set; }
        public float LockWidth { get; set; }
        public float LockSpace { get; set; }
        public int Length { get; set; }

        public readonly PanelLib.PointsStyle Load()
        {
            PanelLib.PointsStyle style = new()
            {
                colorInactive = ColorInactive,
                colorLock = ColorLock,
                lockLength = LockLength,
                lockWidth = LockWidth,
                lockSpace = LockSpace,
                length = Length
            };
            return style;
        }
    }

    public struct JSON_DetectorStyle_Rectangle
    {
        public int MinEdgeMargin { get; set; }
        public int SegmentLength { get; set; }
        public int SegmentSpace { get; set; }
    }

    public struct JSON_DetectorStyle
    {
        public string ID { get; set; }
        public ColorJS ColorEmpty { get; set; }
        public ColorJS ColorOccupied { get; set; }
        public ColorJS ColorOutline { get; set; }
        public float OutlineSize { get; set; }
        public string Style { get; set; }
        public JSON_DetectorStyle_Rectangle? Rectangle { get; set; }

        public readonly PanelLib.DetectorStyle Load()
        {

            PanelLib.DetectorStyle? style = null;

            if (Style == "Rectangle" && Rectangle != null)
            {
                PanelLib.DetectorStyleRectangle rectStyle = new()
                {
                    minEdgeMargin = Rectangle.Value.MinEdgeMargin,
                    segmentLength = Rectangle.Value.SegmentLength,
                    segmentSpace = Rectangle.Value.SegmentSpace
                };

                style = rectStyle;
            }

            if (style == null)
            {
                style = new PanelLib.DetectorStyleRectangle();
            }

            style.outline = OutlineSize;
            style.colorEmpty = ColorEmpty;
            style.colorOccupied = ColorOccupied;
            style.colorOutline = ColorOutline;

            return style;
        }
    }

    public struct JSON_Module_Node
    {
        public string ID { get; set; }
        public int[] Pos { get; set; }

        public readonly PanelLib.TrackNode? Load()
        {
            if (Pos.Length != 2)
                return null;

            PanelLib.TrackNode node = new(ID, Pos[0], Pos[1]);
            return node;
        }
    }

    public struct JSON_Module_Segment
    {
        public string ID { get; set; }
        public string Style { get; set; }
        public string[] Nodes { get; set; }
    }

    public struct JSON_Module_Point
    {
        public string ID { get; set; }
        public string PointsNode { get; set; }
        public string RouteNormal { get; set; }
        public string RouteReversed { get; set; }
        public string Style { get; set; }
        public bool? UseBaseColor { get; set; }
    }

    public struct JSON_Module_Detector
    {
        public string ID { get; set; }
        public string Style { get; set; }
        public string[] Segments { get; set; }
    }

    public struct JSON_Module_DrawingData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public ColorJS BackgroundColor { get; set; }
        public List<JSON_TrackStyle> TrackStyles { get; set; }
        public List<JSON_PointsStyle> PointsStyles { get; set; }
        public List<JSON_DetectorStyle> DetectorStyles { get; set; }
    }

    public struct JSON_Module_TrackData
    {
        public List<JSON_Module_Node> Nodes { get; set; }
        public List<JSON_Module_Segment> Segments { get; set; }
        public List<JSON_Module_Point> Points { get; set; }
        public List<JSON_Module_Detector> Detectors { get; set; }
    }

    public class JSON_StyleData
    {
        public List<JSON_CustomColor>? Colors { get; set; }
        public List<JSON_TrackStyle>? TrackStyles { get; set; }
        public List<JSON_PointsStyle>? PointsStyles { get; set; }
        public List<JSON_DetectorStyle>? DetectorStyles { get; set; }
    }

    public static class JSONLib
    {
        public static void LoadTrackStyles(List<JSON_TrackStyle>? list)
        {
            if (list == null)
                return;

            foreach (JSON_TrackStyle styleData in list)
            {
                if (PanelLib.StyleBank.TrackStyles.HasItem(styleData.ID))
                    throw new Exception("Duplicate track style ID: " + styleData.ID);

                PanelLib.StyleBank.TrackStyles.AddItem(styleData.ID, styleData.Load());
            }
        }

        public static void LoadPointsStyles(List<JSON_PointsStyle>? list)
        {
            if (list == null)
                return;

            foreach (JSON_PointsStyle styleData in list)
            {
                if (PanelLib.StyleBank.PointsStyles.HasItem(styleData.ID))
                    throw new Exception("Duplicate points style ID: " + styleData.ID);

                PanelLib.StyleBank.PointsStyles.AddItem(styleData.ID, styleData.Load());
            }
        }

        public static void LoadDetectorStyles(List<JSON_DetectorStyle>? list)
        {
            if (list == null)
                return;

            foreach (JSON_DetectorStyle styleData in list)
            {
                if (PanelLib.StyleBank.DetectorStyles.HasItem(styleData.ID))
                    throw new Exception("Duplicate track style ID: " + styleData.ID);

                PanelLib.StyleBank.DetectorStyles.AddItem(styleData.ID, styleData.Load());
            }
        }

        public static void LoadStyleFiles(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            string[] allFiles = Directory.GetFiles(dir);
            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) != ".json")
                    continue;

                string json = File.ReadAllText(file);
                JSON_StyleData? styleData = JsonSerializer.Deserialize<JSON_StyleData>(json);
                if (styleData != null)
                {
                    if (styleData.Colors != null)
                    {
                        foreach (JSON_CustomColor color in styleData.Colors)
                        {
                            color.AddToBank();
                        }
                    }
                    
                    LoadTrackStyles(styleData.TrackStyles);
                    LoadPointsStyles(styleData.PointsStyles);
                    LoadDetectorStyles(styleData.DetectorStyles);
                }
            }
        }
    }
}
