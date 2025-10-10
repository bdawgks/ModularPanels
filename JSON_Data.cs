using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public struct JSON_Module_PointsStyle
    {
        public string ID { get; set; }
        public string ColorInactive { get; set; }
        public string ColorLock { get; set; }
        public float LockLength { get; set; }
        public float LockWidth { get; set; }
        public float LockSpace { get; set; }
        public int Length { get; set; }

        public readonly PanelLib.PointsStyle Load()
        {
            PanelLib.PointsStyle style = new()
            {
                colorInactive = Color.FromName(ColorInactive),
                colorLock = Color.FromName(ColorLock),
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

    public struct JSON_Module_DetectorStyle
    {
        public string ID { get; set; }
        public string ColorEmpty { get; set; }
        public string ColorOccupied { get; set; }
        public string ColorOutline { get; set; }
        public float OutlineSize { get; set; }
        public string Style { get; set; }
        public JSON_DetectorStyle_Rectangle? Rectangle { get; set; }

        public readonly PanelLib.DetectorStyle Load()
        {

            PanelLib.DetectorStyle? style = null;

            if (Style == "Rectangle" && Rectangle != null)
            {
                PanelLib.DetectorStyleRectangle rectStyle = new();
                rectStyle.minEdgeMargin = Rectangle.Value.MinEdgeMargin;
                rectStyle.segmentLength = Rectangle.Value.SegmentLength;
                rectStyle.segmentSpace = Rectangle.Value.SegmentSpace;

                style = rectStyle;
            }

            if (style == null)
            {
                style = new PanelLib.DetectorStyleRectangle();
            }

            style.outline = OutlineSize;
            style.colorEmpty = Color.FromName(ColorEmpty);
            style.colorOccupied = Color.FromName(ColorOccupied);
            style.colorOutline = Color.FromName(ColorOutline);

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
        public string BackgroundColor { get; set; }
        public List<JSON_Module_TrackStyle> TrackStyles { get; set; }
        public List<JSON_Module_PointsStyle> PointsStyles { get; set; }
        public List<JSON_Module_DetectorStyle> DetectorStyles { get; set; }

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

        public readonly Dictionary<string, PanelLib.PointsStyle> GetPointsStyles()
        {
            Dictionary<string, PanelLib.PointsStyle> styles = [];
            foreach (JSON_Module_PointsStyle styleData in PointsStyles)
            {
                if (styles.ContainsKey(styleData.ID))
                    throw new Exception("Duplicate points style ID: " + styleData.ID);

                styles.Add(styleData.ID, styleData.Load());
            }
            return styles;
        }

        public readonly Dictionary<string, PanelLib.DetectorStyle> GetDetectorStyles()
        {
            Dictionary<string, PanelLib.DetectorStyle> styles = [];
            foreach (JSON_Module_DetectorStyle styleData in DetectorStyles)
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
        public List<JSON_Module_Detector> Detectors { get; set; }
    }
}
