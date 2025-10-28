using ModularPanels.BlockController;
using ModularPanels.ButtonLib;
using ModularPanels.CircuitLib;
using ModularPanels.Components;
using ModularPanels.DrawLib;
using ModularPanels.JsonLib;
using ModularPanels.PanelLib;
using ModularPanels.SignalLib;
using ModularPanels.TrackLib;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Xml.Linq;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace ModularPanels
{

    public class JSON_Module : IJSONInitializer<Module>
    {
        public string? Name { get; set; }
        public List<CustomColorLoader>? Colors { get; set; }
        public JSON_Module_DrawingData DrawingData { get;set;}
        public TrackDataLoader? TrackData { get;set;}
        public JSON_Module_SignalData SignalData { get;set;}
        public JSON_Module_Controls? Controls { get;set;}
        public CircuitDataLoader? RelayCircuits { get;set;}
        public List<PointsCircuitLoader>? PointsCircuits { get;set;}
        public List<SignalCircuitLoader>? SignalCircuits { get;set;}
        public List<DetectorCircuitLoader>? DetectorCircuits { get;set;}
        public List<BoundaryCircuitLoader>? BoundaryCircuits { get;set;}
        public BlockControllerLoader? BlockController { get;set;}

        public Module Initialize()
        {
            if (Name == null)
                throw new Exception("Module name must be defined!");

            if (Colors != null)
            {
                foreach (CustomColorLoader colorData in Colors)
                {
                    colorData.Load(GlobalBank.Instance);
                }
            }

            if (DrawingData.Width <= 0)
                throw new Exception("Module width must be larger than zero!");

            if (DrawingData.Height <= 0)
                throw new Exception("Module height must be larger than zero!");

            Module module = new(Name, DrawingData.Width, DrawingData.Height)
            {
                BackgroundColor = DrawingData.BackgroundColor
            };

            if (DrawingData.Texts != null)
            {
                foreach (var t in DrawingData.Texts)
                {
                    module.Texts.Add(t.Load());
                }
            }

            if (DrawingData.Rectangles != null)
            {
                foreach (var r in DrawingData.Rectangles)
                {
                    PanelRect? rect = r.Load();
                    if (rect == null)
                        continue;

                    module.Rectangles.Add(rect);
                }
            }

            JSONLib.LoadTrackStyles(DrawingData.TrackStyles);
            JSONLib.LoadPointsStyles(DrawingData.PointsStyles);
            JSONLib.LoadDetectorStyles(DrawingData.DetectorStyles);
            JSONLib.LoadGridStyles(DrawingData.GridStyles);

            if (DrawingData.GridStyle != null)
            {
                GlobalBank.Instance.RegisterKey(DrawingData.GridStyle);
                module.GridStyle = DrawingData.GridStyle.Object;
            }

            TrackData?.Load(module.ObjectBank);

            // TEMPORARY
            //==========
            foreach (var d in module.ObjectBank.GetObjects<TrackDetector>().Values)
            {
                MainWindow.Instance?.AddDetectorDebug(module, d);
            }
            //==========

            SignalData.InitSignals(module);

            if (Controls != null)
                module.AddControls(Controls.Value);

            if (RelayCircuits != null)
            {
                module.GetCircuitComponent().InitCircuits(RelayCircuits);
            }

            if (PointsCircuits != null)
            {
                foreach (var pc in PointsCircuits)
                {
                    pc.Load(module.ObjectBank, module.GetCircuitComponent());
                }
            }

            if (SignalCircuits != null)
            {
                foreach (var sc in SignalCircuits)
                {
                    sc.Load(module.GetSignalComponent(), module.GetCircuitComponent());
                }
            }

            if (DetectorCircuits != null)
            {
                foreach (var dc in DetectorCircuits)
                {
                    dc.Load(module.ObjectBank, module.GetCircuitComponent());
                }
            }

            if (BoundaryCircuits != null)
            {
                foreach (var bc in BoundaryCircuits)
                {
                    bc.Load(module.GetCircuitComponent());
                }
            }

            BlockController?.Load(module);

            return module;
        }
    }

    public class Module: IParent
    {
        readonly string _name;
        readonly int _width;
        readonly int _height;
        readonly ComponentContainer _components;

        Drawing? _drawing;
        Color? _backgroundColor;
        GridStyle? _gridStyle;

        Module? _leftModule;
        Module? _rightModule;

        JSON_Module_Controls? _controlsData;

        readonly ObjectBank _objBank = new();
        readonly Dictionary<string, Signal> _signals = [];
        readonly List<PanelText> _texts = [];
        readonly List<PanelRect> _rectangles = [];
        readonly List<IControl> _allControls = [];

        public string Name
        {
            get => _name;
        }

        public ObjectBank ObjectBank { get { return _objBank; } }
        public Dictionary<string, TrackSegment> TrackSegments { get { return _objBank.GetObjects<TrackSegment>(); } }
        public Dictionary<string, TrackNode> TrackNodes { get { return _objBank.GetObjects<TrackNode>(); } } 
        public Dictionary<string, TrackPoints> TrackPoints { get { return _objBank.GetObjects<TrackPoints>(); } }
        public Dictionary<string, TrackDetector> TrackDetectors { get { return _objBank.GetObjects<TrackDetector>(); } }
        public Dictionary<string, Signal> Signals { get { return _signals; } }
        public List<PanelText> Texts { get { return _texts; } }
        public List<PanelRect> Rectangles { get { return _rectangles; } }

        public Module? LeftModule
        {
            get => _leftModule;
            set => _leftModule = value;
        }

        public Module? RightModule
        {
            get => _rightModule;
            set => _rightModule = value;
        }

        public Module(string name, int width, int height)
        {
            _name = name;
            _width = width;
            _height = height;

            _components = new(this);
        }

        public ComponentContainer Components { get { return _components; } }

        public int Width
        {
            get => _width;
        }

        public int Height
        {
            get => _height;
        }

        public Color? BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public GridStyle? GridStyle
        {
            get => _gridStyle;
            set => _gridStyle = value;
        }

        public void InitDrawing(Drawing drawing)
        {
            _drawing = drawing;
            drawing.Background = BackgroundColor;
            drawing.Border = new()
            {
                color = Color.Black,
                width = 2f
            };

            drawing.GridStyle = _gridStyle;
            foreach (TrackNode n in TrackNodes.Values)
            {
                drawing.AddNode(n);
            }
            foreach (TrackSegment s in TrackSegments.Values)
            {
                drawing.AddSegment(s);
            }
            foreach (TrackPoints p in TrackPoints.Values)
            {
                drawing.AddPoints(p);
            }
            foreach (TrackDetector d in TrackDetectors.Values)
            {
                drawing.AddDetector(d);
            }
            foreach (Signal s in _signals.Values)
            {
                drawing.AddSignal(s);
            }
            foreach (PanelRect r in _rectangles)
            {
                drawing.AddDrawable(r);
                drawing.AddTransformable(r);
            }
            foreach (PanelText t in _texts)
            {
                drawing.AddDrawable(t);
                drawing.AddTransformable(t);
            }
            InitControls();
            foreach (IControl c in _allControls)
            {
                drawing.AddDrawable(c);
                drawing.AddTransformable(c);
            }
        }

        public void AddControls(JSON_Module_Controls controlsData)
        {
            _controlsData = controlsData;
        }

        private InteractionComponent GetInteractionComponent()
        {
            InteractionComponent? component = _components.GetComponent<InteractionComponent>();
            if (component == null)
            {
                component = new(this, MainWindow.Instance!.DrawPanel);
                _components.AddComponent(component);
            }

            return component;
        }

        public CircuitComponent GetCircuitComponent()
        {
            CircuitComponent? component = _components.GetComponent<CircuitComponent>();
            if (component == null)
            {
                component = new(this);
                _components.AddComponent(component);
            }

            return component;
        }

        public SignalComponent GetSignalComponent()
        {
            SignalComponent? component = _components.GetComponent<SignalComponent>();
            if (component == null)
            {
                component = new(this, MainWindow.SignalBank);
                _components.AddComponent(component);
            }

            return component;
        }

        public void InitControls()
        {
            if (_drawing == null || _controlsData == null)
                return;

            if (_controlsData.Value.RotarySwitches != null)
            {
                foreach (var rsData in _controlsData.Value.RotarySwitches)
                {
                    if (TemplateBank<RotarySwitchTemplate>.Instance.TryGetValue(rsData.Template, out RotarySwitchTemplate? template))
                    {
                        RotarySwitch rs = new(GetInteractionComponent(), rsData.Pos, template);
                        if (rsData.SwitchCircuits != null)
                        {
                            foreach (var scData in rsData.SwitchCircuits)
                            {
                                if (GetCircuitComponent().RegisterOrCreateInputCircuit(scData.Circuit, out var ic))
                                    rs.SetActivatedCircuit(scData.Pos, ic);
                            }
                        }
                        if (rsData.LampCircuits != null)
                        {
                            foreach (var scData in rsData.LampCircuits)
                            {
                                GetCircuitComponent().RegisterKey(scData.Circuit);
                                if (scData.Circuit.IsNull)
                                    continue;
                                rs.SetLampActivationCircuit(scData.Pos, scData.Circuit.Object!);
                            }
                        }
                        if (rsData.TextLabels != null)
                        {
                            foreach (var tlData in rsData.TextLabels)
                            {
                                rs.SetTextLabel(tlData.Pos, tlData.Text);
                            }
                        }
                        if (rsData.InterlockCircuit != null)
                        {
                            rs.SetInterlockingCircuit(rsData.InterlockCircuit);
                        }
                        _allControls.Add(rs);
                        rs.Init();
                    }
                }
            }
            if (_controlsData.Value.StateButtons != null)
            {
                foreach (var sbData in _controlsData.Value.StateButtons)
                {
                    if (TemplateBank<StateButtonTemplate>.Instance.TryGetValue(sbData.Template, out StateButtonTemplate? template))
                    {
                        StateButton sb = new(GetInteractionComponent(), sbData.Pos, template);

                        foreach (var sData in sbData.States)
                        {
                            if (sData.CircuitSwitch != null)
                            {
                                GetCircuitComponent().RegisterKey(sData.CircuitSwitch);
                                if (sData.CircuitSwitch.TryGet(out Circuit? circuit))
                                    sb.SetActivationCircuit(sData.State, circuit);
                            }
                            if (sData.CircuitActivated != null)
                            {
                                GetCircuitComponent().RegisterKey(sData.CircuitActivated);
                                if (sData.CircuitActivated.TryGet(out Circuit? circuit) && circuit is InputCircuit ic)
                                    sb.SetActivatedCircuit(sData.State, ic);
                            }
                        }

                        _allControls.Add(sb);
                    }
                }
            }
        }

        public static bool LoadModule(string name, Layout layout, [NotNullWhen(true)] out Module? module)
        {
            module = null;
            string path = Application.StartupPath + "data\\modules\\" + name + ".json";

            if (!File.Exists(path))
                return false;
            try
            {
                string json = File.ReadAllText(path);
                JSON_Module? moduleData = JsonSerializer.Deserialize<JSON_Module>(json);
                if (moduleData == null)
                    return false;

                module = moduleData.Initialize();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                return false;
            }
        }
    }
}
