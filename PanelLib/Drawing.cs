using System.Drawing;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.Versioning;

namespace PanelLib
{
    public enum ShapeMirror
    {
        None,
        AxisX,
        AxisY,
        Both
    }

    public struct Outline
    {
        public Color color;
        public float width;

        public Outline()
        {
            color = Color.Black;
            width = 1.0f;
        }

        public Outline(Color color, float width = 1.0f)
        {
            this.color = color;
            this.width = width;
        }
    }

    public abstract class Shape
    {
        readonly protected Color _fillColor;
        readonly protected Outline _outline;

        public Shape(Color fillColor, Outline outline)
        {
            _fillColor = fillColor;
            _outline = outline;
        }

        public abstract void DrawShape(Graphics g, PointF origin, Vector2 offset, float angle, float scale = 1.0f, ShapeMirror mirror = ShapeMirror.None);
    }

    public class PolygonShape : Shape
    {
        readonly List<Vector2> _verts = new();

        public PolygonShape(int[][] verts, Color fillColor, Outline outline) : base(fillColor, outline)
        {
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].Length == 2)
                {
                    Vector2 vert = new(verts[i][0], verts[i][1]);
                    _verts.Add(vert);
                }
            }
        }

        [SupportedOSPlatform("windows")]
        public override void DrawShape(Graphics g, PointF origin, Vector2 offset, float angle, float scale = 1.0f, ShapeMirror mirror = ShapeMirror.None)
        {
            Brush fillBrush = new SolidBrush(_fillColor);
            Brush outlineBrush = new SolidBrush(_outline.color);
            Pen outlinePen = new(outlineBrush, _outline.width);

            PointF[] verts = Transform(origin, offset, angle, scale, mirror);
            g.FillPolygon(fillBrush, verts);
            g.DrawPolygon(outlinePen, verts);

            fillBrush.Dispose();
            outlineBrush.Dispose();
            outlinePen.Dispose();
        }

        private PointF[] Transform(PointF origin, Vector2 offset, float angle, float scale = 1.0f, ShapeMirror mirror = ShapeMirror.None)
        {
            PointF[] points = new PointF[_verts.Count];

            int i = 0;
            foreach (Vector2 vert in _verts)
            {
                Vector2 mirroredVert = vert;
                if (mirror == ShapeMirror.AxisX || mirror == ShapeMirror.Both)
                {
                    mirroredVert.X = -vert.X;
                }
                if (mirror == ShapeMirror.AxisY || mirror == ShapeMirror.Both)
                {
                    mirroredVert.Y = -vert.Y;
                }
                Vector2 vTransformed = MathLib.RotateVector(mirroredVert * scale, angle);

                points[i++] = new(origin.ToVector2() + offset + vTransformed);
            }

            return points;
        }
    }

    public class EllipseShape : Shape
    {
        readonly Size _size;

        public EllipseShape(Size size, Color fillColor, Outline outline) : base(fillColor, outline)
        {
            _size = size;
        }

        [SupportedOSPlatform("windows")]
        public override void DrawShape(Graphics g, PointF origin, Vector2 offset, float angle, float scale = 1, ShapeMirror mirror = ShapeMirror.None)
        {
            Brush fillBrush = new SolidBrush(_fillColor);
            Brush outlineBrush = new SolidBrush(_outline.color);
            Pen outlinePen = new Pen(outlineBrush, _outline.width);

            Vector2 pos = origin.ToVector2() + offset;
            g.TranslateTransform(pos.X, pos.Y);
            g.RotateTransform(angle);

            Vector2 corner = new Vector2(-_size.Width / 2, -_size.Height / 2);
            RectangleF rect = new RectangleF(new(corner * scale), _size * scale);
            g.FillEllipse(fillBrush, rect);
            g.DrawEllipse(outlinePen, rect);

            g.ResetTransform();

            fillBrush.Dispose();
            outlineBrush.Dispose();
            outlinePen.Dispose();
        }
    }

    public class RectangleShape : Shape
    {
        readonly Point _corner;
        readonly Size _size;

        public RectangleShape(Point corner, Size size, Color fillColor, Outline outline) : base(fillColor, outline)
        {
            _corner = corner;
            _size = size;
        }

        [SupportedOSPlatform("windows")]
        public override void DrawShape(Graphics g, PointF origin, Vector2 offset, float angle, float scale = 1, ShapeMirror mirror = ShapeMirror.None)
        {
            Brush fillBrush = new SolidBrush(_fillColor);
            Brush outlineBrush = new SolidBrush(_outline.color);
            Pen outlinePen = new Pen(outlineBrush, _outline.width);

            Vector2 pos = origin.ToVector2() + offset;
            g.TranslateTransform(pos.X, pos.Y);
            g.RotateTransform(angle);

            PointF cornerScaled = new(_corner.X * scale, _corner.Y * scale);
            RectangleF rect = new RectangleF(cornerScaled, _size * scale);
            g.FillRectangle(fillBrush, rect);
            g.DrawRectangles(outlinePen, [rect]);

            g.ResetTransform();

            fillBrush.Dispose();
            outlineBrush.Dispose();
            outlinePen.Dispose();
        }
    }

    public class ShapeBank
    {
        readonly Dictionary<string, Shape> _shapes = new();

        public void AddShape(string name, Shape shape)
        {
            _shapes.Add(name, shape);
        }

        public Shape? this[string name]
        {
            get
            {
                if (_shapes.TryGetValue(name, out Shape? value))
                    return value;

                return null;
            }
        }
    }

    public struct GridStyle
    {
        public Color gridColor = Color.LightGray;
        public Color textColor = Color.Gray;
        public string textFont = "Calibri";
        public float textSize = 15f;

        public GridStyle() { }
    }

    public struct TrackStyle
    {
        public Color color = Color.Black;
        public float width = 7f;
        public TrackStyle() { }
    }

    public struct PointsStyle
    {
        public Color colorInactive = Color.LightGray;
        public Color colorLock = Color.Blue;
        public float lockLength = 20.0f;
        public float lockWidth = 4.0f;
        public float lockSpace = 3.0f;
        public int length = 35;
        public PointsStyle() { }
    }

    public class Drawing()
    {
        int _gridSize = 5;
        Rectangle _canvas;
        Color? _background;

        GridStyle? _gridStyle;

        readonly List<TrackSegment> _segments = [];
        readonly List<TrackPoints> _points = [];
        readonly List<TrackNode> _nodes = [];
        readonly List<Signal> _signals = [];

        public int GridSize
        {
            get => _gridSize;
            set => _gridSize = value;
        }

        public Rectangle Canvas
        {
            get => _canvas;
            set => _canvas = value;
        }

        public Color? Background
        {
            get => _background;
            set => _background = value;
        }

        public GridStyle? GridStyle
        {
            get => _gridStyle;
            set => _gridStyle = value;
        }

        public void AddSegment(TrackSegment seg)
        {
            _segments.Add(seg);
        }

        public void AddNode(TrackNode node)
        {
            _nodes.Add(node);
        }

        public void AddPoints(TrackPoints points)
        {
            _points.Add(points);
        }

        [SupportedOSPlatform("windows")]
        public void Draw(Graphics g)
        {
            DrawBackground(g);
            DrawGrid(g);

            foreach (TrackSegment segment in _segments)
            {
                DrawTrackSegment(g, segment);
            }
            foreach (TrackPoints p in _points)
            {
                DrawPoints(g, p);
            }
            foreach (TrackNode n in _nodes)
            {
                DrawNode(g, n);
            }
            foreach (Signal sig in _signals)
            {
                DrawSignal(g, sig);
            }
            /*
            foreach (TextLabel label in _labels)
            {
                DrawText(g, label);
            }*/
        }

        private void DrawBackground(Graphics g)
        {
            if (_background == null)
                return;

            Brush bgBrush = new SolidBrush(_background.Value);
            g.FillRectangle(bgBrush, _canvas);
            bgBrush.Dispose();
        }

        [SupportedOSPlatform("windows")]
        private void DrawTrackSegment(Graphics g, TrackSegment segment)
        {
            Brush trackBrush = new SolidBrush(segment.style.color);
            Pen trackPen = new(trackBrush, segment.style.width);
            Point p1 = segment.n0.GetPoint(_gridSize);
            Point p2 = segment.n1.GetPoint(_gridSize);
            Transform(ref p1);
            Transform(ref p2);
            g.DrawLine(trackPen, p1, p2);

            segment.n0.style = segment.style;
            segment.n1.style = segment.style;

            trackBrush.Dispose();
            trackPen.Dispose();
        }

        [SupportedOSPlatform("windows")]
        private void DrawNode(Graphics g, TrackNode node)
        {
            Point p = node.GetPoint(_gridSize);
            Transform(ref p);

            Brush b = new SolidBrush(node.style.color);
            PointF tl = new(p.X - node.style.width * 0.5f, p.Y - node.style.width * 0.5f);
            RectangleF rect = new(tl.X, tl.Y, node.style.width, node.style.width);
            g.FillEllipse(b, rect);

            b.Dispose();
        }

        [SupportedOSPlatform("windows")]
        private void DrawPoints(Graphics g, TrackPoints p)
        {
            Point basePoint = p.baseNode.GetPoint(_gridSize);
            Point normalPoint = p.routeNormal.GetPoint(_gridSize);
            Point reversedPoint = p.routeReversed.GetPoint(_gridSize);
            Transform(ref basePoint);
            Transform(ref normalPoint);
            Transform(ref reversedPoint);
            Vector2 basePos = PointToVector(basePoint);
            Vector2 normalPos = PointToVector(normalPoint);
            Vector2 reversedPos = PointToVector(reversedPoint);

            Vector2 normalDir = Vector2.Normalize(normalPos - basePos);
            Vector2 reversedDir = Vector2.Normalize(reversedPos - basePos);
            normalPos = basePos + normalDir * p.Style.length;
            reversedPos = basePos + reversedDir * p.Style.length;

            normalPoint = VectorToPoint(normalPos);
            reversedPoint = VectorToPoint(reversedPos);

            Point inactivePoint = p.IsReversed ? normalPoint : reversedPoint;
            Point activePoint = p.IsReversed ? reversedPoint : normalPoint;

            Brush inactiveBrush = new SolidBrush(p.Style.colorInactive);
            Pen inactivePen = new(inactiveBrush, p.baseNode.style.width);
            g.DrawLine(inactivePen, basePoint, inactivePoint);

            Brush lockBrush = new SolidBrush(p.Style.colorLock);
            Pen lockPen = new(lockBrush, p.Style.lockWidth);
            if (p.Locked)
            {
                Vector2 pointsDir = p.IsReversed ? reversedDir : normalDir;
                Vector2 pointsPos = p.IsReversed ? reversedPos : normalPos;
                float offsetSpace = (p.baseNode.style.width + p.Style.lockSpace + p.Style.lockWidth) / 2.0f;
                float offsetLength = p.Style.lockLength / 2.0f;
                Vector2 offsetR = MathLib.RotateVector(pointsDir * offsetSpace, 90.0f);
                Vector2 offsetL = MathLib.RotateVector(pointsDir * offsetSpace, -90.0f);
                Vector2 lockLineFR = pointsPos + offsetR + pointsDir * offsetLength;
                Vector2 lockLineBR = pointsPos + offsetR - pointsDir * offsetLength;
                Vector2 lockLineFL = pointsPos + offsetL + pointsDir * offsetLength;
                Vector2 lockLineBL = pointsPos + offsetL - pointsDir * offsetLength;

                g.DrawLine(lockPen, VectorToPoint(lockLineFR), VectorToPoint(lockLineBR));
                g.DrawLine(lockPen, VectorToPoint(lockLineFL), VectorToPoint(lockLineBL));
            }

            TrackStyle trackStyle = p.IsReversed ? p.routeReversed.style : p.routeNormal.style;
            Color pointsColor = trackStyle.color;
            if (p.useBaseColor)
                pointsColor = p.baseNode.style.color;
            Brush activeBrush = new SolidBrush(pointsColor);
            Pen activePen = new(activeBrush, trackStyle.width);
            g.DrawLine(activePen, basePoint, activePoint);

            lockBrush.Dispose();
            lockPen.Dispose();
            inactiveBrush.Dispose();
            inactivePen.Dispose();
            activeBrush.Dispose();
            activePen.Dispose();
        }

        private void DrawSignal(Graphics g, Signal sig)
        {
            if (sig.Type is SignalTypeDraw sType)
            {
                foreach (SignalShape sshape in sType.Shapes)
                {
                    if (sshape.Shape == null)
                        continue;

                    SignalHead? head = sig.GetHead(sshape.HeadID);
                    if (head != null)
                    {
                        if (!sshape.VisibleWithAspect(head.Aspect))
                            continue;
                    }

                    Point pos = new(sig.Pos.X * _gridSize, sig.Pos.Y * _gridSize);
                    Transform(ref pos);
                    sshape.DrawShape(g, pos, sig.Angle, sig.Scale);
                }
            }
        }

        /*
        private void DrawText(Graphics g, TextLabel label)
        {
            FontStyle style = FontStyle.Regular;
            if (label.bold)
                style = FontStyle.Bold;
            Font font = new("Times New Roman", FontSize * label.scale, style);

            StringFormat format = new();
            SizeF stringSize = g.MeasureString(label.text, font, 300 * _gridSize, format);
            PointF corner = new(label.pos.X * _gridSize, label.pos.Y * _gridSize);
            corner.X -= stringSize.Width / 2;
            corner.Y -= stringSize.Height / 2;
            RectangleF rect = new(corner, stringSize);

            Brush textBrush = new SolidBrush(label.color);
            g.DrawString(label.text, font, textBrush, rect);

            format.Dispose();
            font.Dispose();
            textBrush.Dispose();
        }*/

        [SupportedOSPlatform("windows")]
        private void DrawGrid(Graphics g)
        {
            if (_gridStyle == null)
                return;

            List<Point> textPoints = [];
            List<string> textStrings = [];

            Brush gridBrush = new SolidBrush(_gridStyle.Value.gridColor);
            Brush textBrush = new SolidBrush(_gridStyle.Value.textColor);
            Font textFont = new(_gridStyle.Value.textFont, _gridStyle.Value.textSize);

            for (int i = 0; i < _canvas.Height; i++)
            {
                Point p1 = new(0, i * _gridSize);
                Point p2 = new((int)_canvas.Width, i * _gridSize);
                Transform(ref p1);
                Transform(ref p2);
                Pen gridPen = new(gridBrush);
                if (i > 0 && i % 10 == 0)
                {
                    textPoints.Add(p1);
                    textStrings.Add(i.ToString());
                    gridPen = new(textBrush);
                }
                g.DrawLine(gridPen, p1, p2);
                gridPen.Dispose();
            }

            for (int i = 0; i < _canvas.Width; i++)
            {
                Point p1 = new(i * _gridSize, 0);
                Point p2 = new(i * _gridSize, (int)_canvas.Height);
                Transform(ref p1);
                Transform(ref p2);
                Pen gridPen = new(gridBrush);
                if (i > 0 && i % 10 == 0)
                {
                    textPoints.Add(p1);
                    textStrings.Add(i.ToString());
                    gridPen = new(textBrush);
                }
                g.DrawLine(gridPen, p1, p2);
                gridPen.Dispose();
            }

            for (int i = 0; i < textPoints.Count; i++)
            {
                g.DrawString(textStrings[i], textFont, textBrush, textPoints[i]);
            }

            gridBrush.Dispose();
            textBrush.Dispose();
            textFont.Dispose();
        }

        private static Vector2 PointToVector(Point p)
        {
            return new Vector2(p.X, p.Y);
        }
        private static Point VectorToPoint(Vector2 v)
        {
            return new Point((int)Math.Round(v.X), (int)Math.Round(v.Y));
        }
        private void Transform(ref Point p)
        {
            p.X += _canvas.X;
            p.Y += _canvas.Y;
        }
    }
}
