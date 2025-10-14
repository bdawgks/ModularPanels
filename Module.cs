using ModularPanels.ButtonLib;
using ModularPanels.CircuitLib;
using ModularPanels.Components;
using ModularPanels.TrackLib;
using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ModularPanels
{

    public class JSON_Module : IJSONInitializer<Module>
    {
        public string? Name { get; set; }
        public List<JSON_CustomColor>? Colors { get; set; }
        public JSON_Module_DrawingData DrawingData { get;set;}
        public JSON_Module_TrackData TrackData { get;set;}
        public JSON_Module_SignalData SignalData { get;set;}
        public JSON_Module_Controls? Controls { get;set;}
        public JSON_Module_RelayCircuits? RelayCircuits { get;set;}

        private static void GetNode(Module m, string id, out TrackNode? node)
        {
            if (!m.TrackNodes.TryGetValue(id, out node))
                throw new Exception("Could not find node with ID: " + id);
        }

        public Module Initialize()
        {
            if (Name == null)
                throw new Exception("Module name must be defined!");

            if (Colors != null)
            {
                foreach (JSON_CustomColor color in Colors)
                {
                    color.AddToBank();
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

            if (TrackData.Nodes != null && TrackData.Segments != null)
            {
                JSONLib.LoadTrackStyles(DrawingData.TrackStyles);
                foreach (TrackNode node in TrackData.Nodes)
                {
                    module.TrackNodes.Add(node.id, node);
                }

                foreach (JSON_Module_Segment segmentData in TrackData.Segments)
                {
                    if (segmentData.Nodes.Length != 2)
                        throw new Exception("Invalid number of nodes in segment: " + segmentData.Nodes.ToString());

                    GetNode(module, segmentData.Nodes[0], out TrackNode? node0);
                    GetNode(module, segmentData.Nodes[1], out TrackNode? node1);

                    if (node0 == null || node1 == null)
                        continue;

                    if (!DrawLib.StyleBank.TrackStyles.TryGetItem(segmentData.Style, out var trackStyle))
                        trackStyle = new();

                    TrackSegment segment = new(segmentData.ID, trackStyle, node0, node1);
                    module.TrackSegments.Add(segment.id, segment);
                }
            }

            if (TrackData.Points != null)
            {
                JSONLib.LoadPointsStyles(DrawingData.PointsStyles);
                foreach (JSON_Module_Point pointData in TrackData.Points)
                {
                    GetNode(module, pointData.PointsNode, out TrackNode? nodeBase);
                    GetNode(module, pointData.RouteNormal, out TrackNode? nodeNormal);
                    GetNode(module, pointData.RouteReversed, out TrackNode? nodeReversed);

                    if (nodeBase == null || nodeNormal == null || nodeReversed == null)
                        continue;

                    bool useBaseColor = pointData.UseBaseColor != null ? pointData.UseBaseColor.Value : false;

                    if (!DrawLib.StyleBank.PointsStyles.TryGetItem(pointData.Style, out var pointsStyle))
                        throw new Exception("Invalid points style: " + pointData.Style);

                    TrackPoints points = new(pointData.ID, nodeBase, nodeNormal, nodeReversed, useBaseColor);
                    points.Style = pointsStyle;
                    module.TrackPoints.Add(points.id, points);
                }
            }

            if (TrackData.Detectors != null)
            {
                JSONLib.LoadDetectorStyles(DrawingData.DetectorStyles);
                foreach (JSON_Module_Detector detectorData in TrackData.Detectors)
                {
                    TrackDetector detector = new(detectorData.ID);

                    if (!DrawLib.StyleBank.DetectorStyles.TryGetItem(detectorData.Style, out var detectorStyle))
                        throw new Exception("Invalid detector style: " + detectorData.Style);

                    if (detectorStyle != null)
                        detector.Style = detectorStyle;

                    foreach (string segId in detectorData.Segments)
                    {
                        if (!module.TrackSegments.TryGetValue(segId, out TrackSegment seg))
                            throw new Exception("Invalid segment ID: " + segId);

                        detector.AddSegment(seg);
                    }
                    module.TrackDetectors.Add(detector.ID, detector);
                }
            }

            SignalData.InitSignals(module, Layout.SignalSpace);

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
        readonly ComponentContainer _components = new();

        PanelLib.Drawing? _drawing;
        Color? _backgroundColor;

        JSON_Module_Controls? _controlsData;

        readonly Dictionary<string, TrackSegment> _trackSegments = [];
        readonly Dictionary<string, TrackNode> _trackNodes = [];
        readonly Dictionary<string, TrackPoints> _trackPoints = [];
        readonly Dictionary<string, TrackDetector> _trackDetectors = [];
        readonly Dictionary<string, PanelLib.Signal> _signals = [];
        readonly List<PanelLib.PanelText> _texts = [];
        readonly List<IControl> _allControls = [];

        public Dictionary<string, TrackSegment> TrackSegments { get { return _trackSegments; } }
        public Dictionary<string, TrackNode> TrackNodes { get { return _trackNodes; } } 
        public Dictionary<string, TrackPoints> TrackPoints { get { return _trackPoints; } }
        public Dictionary<string, TrackDetector> TrackDetectors { get { return _trackDetectors; } }
        public Dictionary<string, PanelLib.Signal> Signals { get { return _signals; } }
        public List<PanelLib.PanelText> Texts { get { return _texts; } }

        public Module(string name, int width, int height)
        {
            _name = name;
            _width = width;
            _height = height;
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
            foreach (TrackNode n in _trackNodes.Values)
            {
                drawing.AddNode(n);
            }
            foreach (TrackSegment s in _trackSegments.Values)
            {
                drawing.AddSegment(s);
            }
            foreach (TrackPoints p in _trackPoints.Values)
            {
                drawing.AddPoints(p);
            }
            foreach (TrackDetector d in _trackDetectors.Values)
            {
                drawing.AddDetector(d);
            }
            foreach (PanelLib.Signal s in _signals.Values)
            {
                drawing.AddSignal(s);
            }
            foreach (PanelLib.PanelText t in _texts)
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
