using PanelLib;
using System.Text.Json;
using ModularPanels.DrawLib;
using ModularPanels.TrackLib;

namespace ModularPanels
{
    public interface IJSONInitializer<T>
    {
        public T Initialize();
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
            if (DrawLib.CustomColorBank.Instance.HasColor(Name))
                throw new Exception("Color with given name was already defined: " + Name);

            DrawLib.CustomColorBank.Instance.AddColor(Name, GetColor());
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
        public int Width { get; set; }
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
                    segmentSpace = Rectangle.Value.SegmentSpace,
                    width = Rectangle.Value.Width
                };

                style = rectStyle;
            }

            style ??= new PanelLib.DetectorStyleRectangle();

            style.outline = OutlineSize;
            style.colorEmpty = ColorEmpty;
            style.colorOccupied = ColorOccupied;
            style.colorOutline = ColorOutline;

            return style;
        }
    }

    public struct JSON_TextStyle
    {
        public string ID { get; set; }
        public string Font { get; set; }
        public int Size { get; set; }
        public ColorJS Color { get; set; }
        public readonly PanelLib.TextStyle Load()
        {

            PanelLib.TextStyle style = new()
            {
                font = Font,
                size = Size,
                color = Color
            };

            return style;
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

    public struct JSON_Text
    {
        public string Text { get; set; }
        public string Style { get; set; }
        public int[] Pos { get; set; }
        public float? Angle { get; set; }

        public readonly PanelLib.PanelText Load()
        {
            if (Pos.Length != 2)
                throw new Exception("Invalid position for text " + Text);

            float angle = 0f;
            if (Angle != null)
                angle = Angle.Value;

            if (!DrawLib.StyleBank.TextStyles.TryGetItem(Style, out TextStyle style))
                style = new();

            PanelLib.PanelText text = new(Text, Pos[0], Pos[1], angle)
            {
                Text = Text,
                Style = style
            };
            return text;
        }
    }

    public struct JSON_Module_DrawingData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public ColorJS BackgroundColor { get; set; }
        public List<JSON_Text> Texts { get; set; }
        public List<JSON_TrackStyle> TrackStyles { get; set; }
        public List<JSON_PointsStyle> PointsStyles { get; set; }
        public List<JSON_DetectorStyle> DetectorStyles { get; set; }
    }

    public struct JSON_Module_TrackData
    {
        public List<TrackNode> Nodes { get; set; }
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
        public List<JSON_TextStyle>? TextStyles { get; set; }
    }

    public struct JSON_Module_Signal
    {
        public string ID { get; set; }
        public string Type { get; set; }
        public int[] Pos { get; set; }
        public float? Angle { get; set; }
        public float? Scale { get; set; }
    }

    public struct JSON_Module_SignalData
    {
        public List<JSON_Module_Signal> Signals { get; set; }

        public readonly void InitSignals(Module mod, SignalSpace space)
        {
            if (Signals == null)
                return;

            foreach (var signal in Signals)
            {
                Signal? sig = space.CreateSignal(signal.ID, signal.Type);
                if (sig == null)
                    continue;

                sig.SetPos(signal.Pos);
                if (signal.Angle != null)
                    sig.SetAngle(signal.Angle.Value);
                if (signal.Scale != null)
                    sig.SetScale(signal.Scale.Value);

                mod.Signals.Add(sig.Name, sig);
                sig.InitSignal();
            }
        }
    }

    public struct JSON_Module_SwitchCircuit
    {
        public int Pos { get; set; }
        public string Circuit { get; set; }
    }

    public struct JSON_Module_SwitchTextLabels
    {
        public int Pos { get; set; }
        public string Text { get; set; }
    }

    public struct JSON_Module_ControlRotarySwitch
    {
        public string ID { get; set; }
        public GridPos Pos { get; set; }
        public string Template { get; set; }
        public List<JSON_Module_SwitchCircuit> SwitchCircuits { get; set; }
        public List<JSON_Module_SwitchCircuit> LampCircuits { get; set; }
        public List<JSON_Module_SwitchTextLabels> TextLabels { get; set; }
        public string? InterlockCircuit { get; set; }
    }

    public struct JSON_Module_Controls
    {
        public List<JSON_Module_ControlRotarySwitch> RotarySwitches { get; set; }
    }

    public struct JSON_Module_SimpleCircuit
    {
        public string ID { get; set; }
        public string Desc { get; set; }
        public bool Active { get; set; }
    }

    public struct JSON_Module_LogicOperator
    {
        public string Op { get; set; }
        public string Circuit { get; set; }
    }

    public struct JSON_Module_LogicCircuit
    {
        public string ID { get; set; }
        public List<JSON_Module_LogicOperator> Condition { get; set; }
        public List<JSON_Module_LogicOperator> ConditionOn { get; set; }
        public List<JSON_Module_LogicOperator> ConditionOff { get; set; }
        public string Desc { get; set; }
    }

    public struct JSON_Module_SignalCircuit
    {
        public string SigID { get; set; }
        public string Circuit { get; set; }
        public string Indication { get; set; }
    }

    public struct JSON_Module_RelayCircuits
    {
        public List<JSON_Module_SimpleCircuit> SimpleCircuits { get; set; }
        public List<JSON_Module_LogicCircuit> LogicCircuits { get; set; }
        public List<JSON_Module_SignalCircuit> SignalCircuits { get; set; }
    }

    public static class JSONLib
    {
        public static void LoadTrackStyles(List<JSON_TrackStyle>? list)
        {
            if (list == null)
                return;

            foreach (JSON_TrackStyle styleData in list)
            {
                if (DrawLib.StyleBank.TrackStyles.HasItem(styleData.ID))
                    throw new Exception("Duplicate track style ID: " + styleData.ID);

                DrawLib.StyleBank.TrackStyles.AddItem(styleData.ID, styleData.Load());
            }
        }

        public static void LoadPointsStyles(List<JSON_PointsStyle>? list)
        {
            if (list == null)
                return;

            foreach (JSON_PointsStyle styleData in list)
            {
                if (DrawLib.StyleBank.PointsStyles.HasItem(styleData.ID))
                    throw new Exception("Duplicate points style ID: " + styleData.ID);

                DrawLib.StyleBank.PointsStyles.AddItem(styleData.ID, styleData.Load());
            }
        }

        public static void LoadDetectorStyles(List<JSON_DetectorStyle>? list)
        {
            if (list == null)
                return;

            foreach (JSON_DetectorStyle styleData in list)
            {
                if (DrawLib.StyleBank.DetectorStyles.HasItem(styleData.ID))
                    throw new Exception("Duplicate track style ID: " + styleData.ID);

                DrawLib.StyleBank.DetectorStyles.AddItem(styleData.ID, styleData.Load());
            }
        }

        public static void LoadTextStyles(List<JSON_TextStyle>? list)
        {
            if (list == null)
                return;

            foreach (JSON_TextStyle styleData in list)
            {
                if (DrawLib.StyleBank.TextStyles.HasItem(styleData.ID))
                    throw new Exception("Duplicate text style ID: " + styleData.ID);

                DrawLib.StyleBank.TextStyles.AddItem(styleData.ID, styleData.Load());
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
                    LoadTextStyles(styleData.TextStyles);
                }
            }
        }

        public static void LoadSignalFiles(PanelLib.SignalSpace sigSpace, string dir)
        {
            if (!Directory.Exists(dir))
                return;

            string[] allFiles = Directory.GetFiles(dir);
            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) != ".json")
                    continue;

                PanelLib.JSONData.JSONLoader.LoadSignalLibrary(sigSpace, file);
            }
        }
    }
}
