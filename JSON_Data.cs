using ModularPanels.DrawLib;
using ModularPanels.JsonLib;
using ModularPanels.PanelLib;
using ModularPanels.SignalLib;
using ModularPanels.TrackLib;
using ModularPanels.CircuitLib;
using System.Text.Json;

namespace ModularPanels
{
    public interface IJSONInitializer<T>
    {
        public T Initialize();
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
        public GridPos Pos { get; set; }
        public float? Angle { get; set; }

        public readonly PanelText Load()
        {
            float angle = 0f;
            if (Angle != null)
                angle = Angle.Value;

            StringKey<TextStyle> id = new(Style);
            GlobalBank.Instance.RegisterKey(id);
            TextStyle style;
            if (id.IsNull)
                style = new();
            else
                style = id.Object!;

                PanelText text = new(Pos, angle)
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
        public StringKey<GridStyle>? GridStyle { get; set; }
        public List<JSON_Text> Texts { get; set; }
        public List<PanelRectLoader> Rectangles { get; set; }
        public List<TrackStyleLoader> TrackStyles { get; set; }
        public List<PointsStyleLoader> PointsStyles { get; set; }
        public List<DetectorStyleLoader> DetectorStyles { get; set; }
        public List<GridStyleLoader> GridStyles { get; set; }
    }

    public class JSON_StyleData
    {
        public List<CustomColorLoader>? Colors { get; set; }
        public List<TrackStyleLoader>? TrackStyles { get; set; }
        public List<PointsStyleLoader>? PointsStyles { get; set; }
        public List<DetectorStyleLoader>? DetectorStyles { get; set; }
        public List<TextStyleLoader>? TextStyles { get; set; }
        public List<GridStyleLoader>? GridStyles { get; set; }
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
        public List<BoundarySignalLoader>? BoundarySignals { get; set; }
        public List<SignalRouteLoader>? SignalRoutes { get; set; }

        public readonly void InitSignals(Module mod)
        {
            if (Signals == null)
                return;

            if (!mod.Components.CreateComponent([MainWindow.SignalBank], out SignalComponent? comp))
                return;

            foreach (var signal in Signals)
            {
                Signal? sig = comp.CreateSignal(signal.ID, signal.Type);
                if (sig == null)
                    continue;

                sig.SetPos(signal.Pos);
                if (signal.Angle != null)
                    sig.SetAngle(signal.Angle.Value);
                if (signal.Scale != null)
                    sig.SetScale(signal.Scale.Value);

                mod.Signals.Add(sig.Name, sig);
            }

            if (BoundarySignals != null)
            {
                foreach (var boundarySig in BoundarySignals)
                {
                    boundarySig.Load(mod);
                }
            }

            if (SignalRoutes != null)
            {
                foreach (var sr in SignalRoutes)
                {
                    sr.Load(mod.GetSignalComponent(), mod.ObjectBank);
                }
            }

            mod.GetSignalComponent().InitSignals();
        }
    }

    public struct JSON_Module_SwitchCircuit
    {
        public int Pos { get; set; }
        public StringKey<Circuit> Circuit { get; set; }
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

    public struct JSON_Module_ControlStateButtonState
    {
        public int State { get; set; }
        public StringKey<Circuit>? CircuitSwitch { get; set; }
        public StringKey<Circuit>? CircuitActivated { get; set; }
    }

    public struct JSON_Module_ControlStateButton
    {
        public string ID { set; get; }
        public GridPos Pos { get; set; }
        public string Template { get; set; }
        public List<JSON_Module_ControlStateButtonState> States { get; set; }
    }

    public struct JSON_Module_Controls
    {
        public List<JSON_Module_ControlRotarySwitch> RotarySwitches { get; set; }
        public List<JSON_Module_ControlStateButton> StateButtons { get; set; }
    }

    public static class JSONLib
    {
        public static void LoadTrackStyles(List<TrackStyleLoader>? list)
        {
            if (list == null)
                return;

            foreach (TrackStyleLoader styleData in list)
            {
                styleData.Load(GlobalBank.Instance);
            }
        }

        public static void LoadPointsStyles(List<PointsStyleLoader>? list)
        {
            if (list == null)
                return;

            foreach (PointsStyleLoader styleData in list)
            {
                styleData.Load(GlobalBank.Instance);
            }
        }

        public static void LoadDetectorStyles(List<DetectorStyleLoader>? list)
        {
            if (list == null)
                return;

            foreach (DetectorStyleLoader styleData in list)
            {
                styleData.Load(GlobalBank.Instance);
            }
        }

        public static void LoadTextStyles(List<TextStyleLoader>? list)
        {
            if (list == null)
                return;

            foreach (TextStyleLoader styleData in list)
            {
                styleData.Load(GlobalBank.Instance);
            }
        }

        public static void LoadGridStyles(List<GridStyleLoader>? list)
        {
            if (list == null)
                return;

            foreach (GridStyleLoader styleData in list)
            {
                styleData.Load(GlobalBank.Instance);
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
                        foreach (CustomColorLoader colorData in styleData.Colors)
                        {
                            colorData.Load(JsonLib.GlobalBank.Instance);
                        }
                    }
                    
                    LoadTrackStyles(styleData.TrackStyles);
                    LoadPointsStyles(styleData.PointsStyles);
                    LoadDetectorStyles(styleData.DetectorStyles);
                    LoadTextStyles(styleData.TextStyles);
                    LoadGridStyles(styleData.GridStyles);
                }
            }
        }

        public static void LoadSignalFiles(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            string[] allFiles = Directory.GetFiles(dir);
            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) != ".json")
                    continue;

                string json = File.ReadAllText(file);
                SignalLibraryLoader? lib = JsonSerializer.Deserialize<SignalLibraryLoader>(json);

                lib?.Load(MainWindow.SignalBank);
            }

            MainWindow.SignalBank.InitShapes();
        }
    }
}
