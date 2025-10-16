using ModularPanels.ButtonLib;
using ModularPanels.CircuitLib;
using ModularPanels.Components;
using ModularPanels.JsonLib;
using ModularPanels.TrackLib;
using ModularPanels.PanelLib;
using ModularPanels.DrawLib;
using ModularPanels.SignalLib;
using System.Text.Json;

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
        public JSON_Module_RelayCircuits? RelayCircuits { get;set;}

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

            JSONLib.LoadTrackStyles(DrawingData.TrackStyles);
            JSONLib.LoadPointsStyles(DrawingData.PointsStyles);
            JSONLib.LoadDetectorStyles(DrawingData.DetectorStyles);

            if (TrackData != null)
            {
                TrackData.Load(module.ObjectBank);
            }

            SignalData.InitSignals(module);

            if (Controls != null)
                module.AddControls(Controls.Value);

            if (RelayCircuits != null)
            {
                module.GetCircuitComponent().InitCircuits(RelayCircuits.Value);
            }

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

        JSON_Module_Controls? _controlsData;

        readonly ObjectBank _objBank = new();
        readonly Dictionary<string, Signal> _signals = [];
        readonly List<PanelText> _texts = [];
        readonly List<IControl> _allControls = [];

        public ObjectBank ObjectBank { get { return _objBank; } }
        public Dictionary<string, TrackSegment> TrackSegments { get { return _objBank.GetObjects<TrackSegment>(); } }
        public Dictionary<string, TrackNode> TrackNodes { get { return _objBank.GetObjects<TrackNode>(); } } 
        public Dictionary<string, TrackPoints> TrackPoints { get { return _objBank.GetObjects<TrackPoints>(); } }
        public Dictionary<string, TrackDetector> TrackDetectors { get { return _objBank.GetObjects<TrackDetector>(); } }
        public Dictionary<string, Signal> Signals { get { return _signals; } }
        public List<PanelText> Texts { get { return _texts; } }

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

        public void InitDrawing(Drawing drawing)
        {
            _drawing = drawing;
            drawing.Background = BackgroundColor;
            drawing.Border = new()
            {
                color = Color.Black,
                width = 2f
            };

            //drawing.GridStyle = new PanelLib.GridStyle()
            //{
            //    majorColor = Color.Black,
            //    minorColor = Color.WhiteSmoke,
            //    textColor = Color.Black
            //};

            //drawing.GridStyle = new PanelLib.GridStyle()
            //{
            //    majorColor = Color.DarkGreen,
            //    minorColor = Color.Empty,
            //    textColor = Color.Empty
            //};
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
            foreach (PanelText t in _texts)
            {
                drawing.AddText(t);
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
                                rs.SetActivatedCircuit(scData.Pos, scData.Circuit);
                            }
                        }
                        if (rsData.LampCircuits != null)
                        {
                            foreach (var scData in rsData.LampCircuits)
                            {
                                rs.SetLampActivationCircuit(scData.Pos, scData.Circuit);
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
                    }
                }
            }
        }

        public static bool LoadModule(string name, Layout layout, out Module? module)
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
