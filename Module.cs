using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ModularPanels
{
    public interface IJSONInitializer<T>
    {
        public T Initialize();
    }

    public struct JSON_Module_TrackStyle
    {
        public string ID { get; set; }
        public string Color { get; set; }
        public float Width { get; set; }

        public readonly PanelLib.TrackStyle Load()
        {
            PanelLib.TrackStyle style = new()
            {
                color = System.Drawing.Color.FromName(Color),
                width = Width
            };
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
        public string Style { get; set; }
        public string[] Nodes { get; set; }
    }

    public struct JSON_Module_Point
    {
        public string ID { get; set; }
        public string PointsNode { get; set; }
        public string RouteNormal { get; set; }
        public string RouteReversed { get; set; }
        public bool? UseBaseColor { get; set; }
    }

    public struct JSON_Module_DrawingData
    {
        public int Width { get; set; }
        public string BackgroundColor { get; set; }
        public List<JSON_Module_TrackStyle> TrackStyles { get; set; }

        public readonly Dictionary<string, PanelLib.TrackStyle> GetTrackStyles()
        {
            Dictionary<string, PanelLib.TrackStyle> styles = [];
            foreach (JSON_Module_TrackStyle styleData in TrackStyles)
            {
                if (styles.ContainsKey(styleData.ID))
                    throw new Exception("Duplicate track style ID: " + styleData.ID);

                styles.Add(styleData.ID, styleData.Load());
            }
            return styles;
        }
    }

    public struct JSON_Module_TrackData
    {
        public List<JSON_Module_Node> Nodes { get; set; }
        public List<JSON_Module_Segment> Segments { get; set; }
        public List<JSON_Module_Point> Points { get; set; }
    }

    public class JSON_Module : IJSONInitializer<Module>
    {
        public string? Name { get; set; }
        public JSON_Module_DrawingData DrawingData { get;set;}
        public JSON_Module_TrackData TrackData { get;set;}

        private static void GetNode(Module m, string id, out PanelLib.TrackNode? node)
        {
            if (!m.TrackNodes.TryGetValue(id, out node))
                throw new Exception("Could not find node with ID: " + id);
        }

        public Module Initialize()
        {
            if (Name == null)
                throw new Exception("Module name must be defined!");

            if (DrawingData.Width <= 0)
                throw new Exception("Module width must be larger than zero!");

            Module module = new(Name, DrawingData.Width);
            if (DrawingData.BackgroundColor != string.Empty)
                module.BackgroundColor = Color.FromName(DrawingData.BackgroundColor);

            var trackStyles = DrawingData.GetTrackStyles();
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

                if (!trackStyles.TryGetValue(segmentData.Style, out var trackStyle))
                    trackStyle = new();

                PanelLib.TrackSegment segment = new(trackStyle, node0, node1);
                module.TrackSegments.Add(segment);
            }
            foreach (JSON_Module_Point pointData in TrackData.Points)
            {
                GetNode(module, pointData.PointsNode, out PanelLib.TrackNode? nodeBase);
                GetNode(module, pointData.RouteNormal, out PanelLib.TrackNode? nodeNormal);
                GetNode(module, pointData.RouteReversed, out PanelLib.TrackNode? nodeReversed);

                if (nodeBase == null || nodeNormal == null || nodeReversed == null)
                    continue;

                bool useBaseColor = pointData.UseBaseColor != null ? pointData.UseBaseColor.Value : false;

                PanelLib.TrackPoints points = new(pointData.ID, nodeBase, nodeNormal, nodeReversed, useBaseColor);
                module.TrackPoints.Add(points.id, points);
            }

            return module;
        }
    }

    public class Module
    {
        readonly string _name;
        readonly int _width;

        Color? _backgroundColor;

        readonly List<TrackSegment> _trackSegments = [];
        readonly Dictionary<string, TrackNode> _trackNodes = [];
        readonly Dictionary<string, TrackPoints> _trackPoints = [];
        readonly Dictionary<string, TrackDetector> _trackDetectors = [];
        readonly Dictionary<string, Signal> _signals = [];

        public List<TrackSegment> TrackSegments { get { return _trackSegments; } }
        public Dictionary<string, TrackNode> TrackNodes { get { return _trackNodes; } } 
        public Dictionary<string, TrackPoints> TrackPoints { get { return _trackPoints; } }
        public Dictionary<string, TrackDetector> TrackDetectors { get { return _trackDetectors; } }

        public Module(string name, int width)
        {
            _name = name;
            _width = width;
        }

        public Color? BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public void InitDrawing(Drawing drawing)
        {
            drawing.Background = BackgroundColor;
            foreach (TrackNode n in _trackNodes.Values)
            {
                drawing.AddNode(n);
            }
            foreach (TrackSegment s in _trackSegments)
            {
                drawing.AddSegment(s);
            }
            foreach (TrackPoints p in _trackPoints.Values)
            {
                drawing.AddPoints(p);
            }
        }
    }
}
