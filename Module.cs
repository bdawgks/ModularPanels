using ModularPanels.ButtonLib;
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

        private static void GetNode(Module m, string id, out PanelLib.TrackNode? node)
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
                foreach (JSON_Module_Node nodeData in TrackData.Nodes)
                {
                    PanelLib.TrackNode? node = nodeData.Load();
                    if (node != null)
                    {
                        module.TrackNodes.Add(node.id, node);
                    }
                }

                foreach (JSON_Module_Segment segmentData in TrackData.Segments)
                {
                    if (segmentData.Nodes.Length != 2)
                        throw new Exception("Invalid number of nodes in segment: " + segmentData.Nodes.ToString());

                    GetNode(module, segmentData.Nodes[0], out PanelLib.TrackNode? node0);
                    GetNode(module, segmentData.Nodes[1], out PanelLib.TrackNode? node1);

                    if (node0 == null || node1 == null)
                        continue;

                    if (!PanelLib.StyleBank.TrackStyles.TryGetItem(segmentData.Style, out var trackStyle))
                        trackStyle = new();

                    PanelLib.TrackSegment segment = new(segmentData.ID, trackStyle, node0, node1);
                    module.TrackSegments.Add(segment.id, segment);
                }
            }

            if (TrackData.Points != null)
            {
                JSONLib.LoadPointsStyles(DrawingData.PointsStyles);
                foreach (JSON_Module_Point pointData in TrackData.Points)
                {
                    GetNode(module, pointData.PointsNode, out PanelLib.TrackNode? nodeBase);
                    GetNode(module, pointData.RouteNormal, out PanelLib.TrackNode? nodeNormal);
                    GetNode(module, pointData.RouteReversed, out PanelLib.TrackNode? nodeReversed);

                    if (nodeBase == null || nodeNormal == null || nodeReversed == null)
                        continue;

                    bool useBaseColor = pointData.UseBaseColor != null ? pointData.UseBaseColor.Value : false;

                    if (!PanelLib.StyleBank.PointsStyles.TryGetItem(pointData.Style, out var pointsStyle))
                        throw new Exception("Invalid points style: " + pointData.Style);

                    PanelLib.TrackPoints points = new(pointData.ID, nodeBase, nodeNormal, nodeReversed, useBaseColor);
                    points.Style = pointsStyle;
                    module.TrackPoints.Add(points.id, points);
                }
            }

            if (TrackData.Detectors != null)
            {
                JSONLib.LoadDetectorStyles(DrawingData.DetectorStyles);
                foreach (JSON_Module_Detector detectorData in TrackData.Detectors)
                {
                    PanelLib.TrackDetector detector = new(detectorData.ID);

                    if (!PanelLib.StyleBank.DetectorStyles.TryGetItem(detectorData.Style, out var detectorStyle))
                        throw new Exception("Invalid detector style: " + detectorData.Style);

                    if (detectorStyle != null)
                        detector.Style = detectorStyle;

                    foreach (string segId in detectorData.Segments)
                    {
                        if (!module.TrackSegments.TryGetValue(segId, out PanelLib.TrackSegment seg))
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
                MainWindow.Instance!.ISpace.InitCircuits(RelayCircuits.Value);

            return module;
        }
    }

    public class Module
    {
        readonly string _name;
        readonly int _width;
        readonly int _height;

        PanelLib.Drawing? _drawing;
        Color? _backgroundColor;

        JSON_Module_Controls? _controlsData;

        readonly Dictionary<string, PanelLib.TrackSegment> _trackSegments = [];
        readonly Dictionary<string, PanelLib.TrackNode> _trackNodes = [];
        readonly Dictionary<string, PanelLib.TrackPoints> _trackPoints = [];
        readonly Dictionary<string, PanelLib.TrackDetector> _trackDetectors = [];
        readonly Dictionary<string, PanelLib.Signal> _signals = [];
        readonly List<PanelLib.PanelText> _texts = [];

        public Dictionary<string, PanelLib.TrackSegment> TrackSegments { get { return _trackSegments; } }
        public Dictionary<string, PanelLib.TrackNode> TrackNodes { get { return _trackNodes; } } 
        public Dictionary<string, PanelLib.TrackPoints> TrackPoints { get { return _trackPoints; } }
        public Dictionary<string, PanelLib.TrackDetector> TrackDetectors { get { return _trackDetectors; } }
        public Dictionary<string, PanelLib.Signal> Signals { get { return _signals; } }
        public List<PanelLib.PanelText> Texts { get { return _texts; } }

        public Module(string name, int width, int height)
        {
            _name = name;
            _width = width;
            _height = height;
        }

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
            foreach (PanelLib.TrackNode n in _trackNodes.Values)
            {
                drawing.AddNode(n);
            }
            foreach (PanelLib.TrackSegment s in _trackSegments.Values)
            {
                drawing.AddSegment(s);
            }
            foreach (PanelLib.TrackPoints p in _trackPoints.Values)
            {
                drawing.AddPoints(p);
            }
            foreach (PanelLib.TrackDetector d in _trackDetectors.Values)
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
        }

        public void AddControls(JSON_Module_Controls controlsData)
        {
            _controlsData = controlsData;
        }

        public void InitControls()
        {
            if (_drawing == null || _controlsData == null)
                return;

            if (_controlsData.Value.RotarySwitches != null)
            {
                foreach (var rsData in _controlsData.Value.RotarySwitches)
                {
                    Point pos = new(rsData.Pos[0], rsData.Pos[1]);
                    _drawing.Transform(ref pos);

                    if (TemplateBank<RotarySwitchTemplate>.Instance.TryGetValue(rsData.Template, out RotarySwitchTemplate? template))
                    {
                        RotarySwitch rs = new(MainWindow.Instance!.ISpace, pos, template);
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
