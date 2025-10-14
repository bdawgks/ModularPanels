using ModularPanels;
using ModularPanels.DrawLib;
using ModularPanels.TrackLib;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms.Design.Behavior;

namespace ModularPanels.PanelLib
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
        public Color majorColor = Color.LightGray;
        public Color minorColor = Color.LightGray;
        public Color textColor = Color.Gray;
        public string textFont = "Calibri";
        public float textSize = 15f;

        public GridStyle() { }
    }

    public class TextStyle
    {
        public string font = "Calibri";
        public Color color = Color.Black;
        public int size = 30;
        public bool bold = false;
    }

    public class PanelText
    {
        readonly string _id;
        readonly int _x;
        readonly int _y;
        readonly float _angle = 0f;

        string _text = "";
        TextStyle _style = new();

        public string Text
        {
            get => _text; 
            set => _text = value;
        }

        public float Angle
        {
            get => _angle;
        }

        public TextStyle Style
        {
            get => _style;
            set => _style = value;
        }

        public PanelText(string id, int x, int y, float angle)
        {
            _id = id;
            _x = x;
            _y = y;
            _angle = angle;
        }

        public Point GetPoint(int gridSize)
        {
            return new(_x * gridSize, _y * gridSize);
        }
    }

    public struct DrawingContext
    {
        public Drawing drawing;
        public Graphics graphics;
    }

    public struct DrawingBorder
    {
        public Color color;
        public float width = 2f;

        public DrawingBorder() {}
    }

    public class Drawing()
    {
        int _gridSize = 5;
        Rectangle _canvas;
        Color? _background;
        DrawingBorder? _border;

        GridStyle? _gridStyle;

        readonly List<TrackSegment> _segments = [];
        readonly List<TrackPoints> _points = [];
        readonly List<TrackNode> _nodes = [];
        readonly List<TrackDetector> _detectors = [];
        readonly List<Signal> _signals = [];
        readonly List<PanelText> _texts = [];
        readonly List<IDrawable> _drawables = [];

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

        public DrawingBorder? Border
        {
            get => _border;
            set => _border = value;
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

        public void AddDetector(TrackDetector detector)
        {
            _detectors.Add(detector);
        }

        public void AddSignal(Signal signal)
        {
            _signals.Add(signal);
        }

        public void AddText(PanelText text)
        {
            _texts.Add(text);
        }
        public void AddDrawable(IDrawable drawable)
        {
            _drawables.Add(drawable);
        }
        public void AddTransformable(IDrawTransformable transformable)
        {
            foreach (var t in transformable.GetTransforms())
            {
                t.SetDrawing(this);
            }
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
            foreach (TrackDetector d in _detectors)
            {
                DrawDetector(g, d);
            }
            foreach (Signal sig in _signals)
            {
                DrawSignal(g, sig);
            }
            foreach (PanelText t in _texts)
            {
                DrawText(g, t);
            }

            DrawingContext context = new()
            {
                graphics = g,
                drawing = this
            };
            foreach (IDrawable d in _drawables)
            {
                d.Draw(context);
            }

            DrawBorder(g);
        }

        private void DrawBackground(Graphics g)
        {
            if (_background == null)
                return;

            Brush bgBrush = new SolidBrush(_background.Value);
            g.FillRectangle(bgBrush, _canvas);
            bgBrush.Dispose();
        }

        private void DrawBorder(Graphics g)
        {
            if (_border == null)
                return;

            Brush borderBrush = new SolidBrush(_border.Value.color);
            Pen borderPen = new(borderBrush, _border.Value.width);
            g.DrawRectangle(borderPen, _canvas);
            borderBrush.Dispose();
            borderPen.Dispose();
        }

        [SupportedOSPlatform("windows")]
        private void DrawTrackSegment(Graphics g, TrackSegment segment)
        {
            Brush trackBrush = new SolidBrush(segment.style.color);
            Pen trackPen = new(trackBrush, segment.style.width);
            Point p1 = Transform(segment.n0.pos.ToDrawingPos());
            Point p2 = Transform(segment.n1.pos.ToDrawingPos());
            g.DrawLine(trackPen, p1, p2);

            segment.n0.style = segment.style;
            segment.n1.style = segment.style;

            Vector2 v0 = PointToVector(p1);
            Vector2 v1 = PointToVector(p2);
            float angle = VectorAngle(v1 - v0);
            segment.n0.segDir = angle;
            segment.n1.segDir = angle;

            trackBrush.Dispose();
            trackPen.Dispose();
        }

        [SupportedOSPlatform("windows")]
        private void DrawNode(Graphics g, TrackNode node)
        {
            Point p = Transform(node.pos.ToDrawingPos());

            Brush b = new SolidBrush(node.style.color);
            PointF tl = new(p.X - node.style.width * 0.5f, p.Y - node.style.width * 0.5f);
            RectangleF rect = new(tl.X, tl.Y, node.style.width, node.style.width);
            if (node.squareEnd)
            {
                g.TranslateTransform(p.X, p.Y);
                rect.X -= p.X;
                rect.Y -= p.Y;
                g.RotateTransform(node.segDir);
                g.FillRectangle(b, rect);
                g.ResetTransform();
            }
            else
                g.FillEllipse(b, rect);

            b.Dispose();
        }

        [SupportedOSPlatform("windows")]
        private void DrawPoints(Graphics g, TrackPoints p)
        {
            DrawingPos baseDPos = p.baseNode.pos.ToDrawingPos();
            DrawingPos normalDPos = p.routeNormal.pos.ToDrawingPos();
            DrawingPos reverseDPos = p.routeReversed.pos.ToDrawingPos();
            Point basePoint = Transform(baseDPos);
            Point normalPoint = Transform(normalDPos);
            Point reversedPoint = Transform(reverseDPos);
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
            foreach (SignalShape sshape in sig.Type.Shapes)
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

        public void DrawText(Graphics g, string text, Point pos, TextStyle style, float angle = 0f)
        {
            FontStyle fontStyle = FontStyle.Regular;
            if (style.bold)
                fontStyle = FontStyle.Bold;
            Font font = new("Times New Roman", style.size, fontStyle);

            g.TranslateTransform(pos.X, pos.Y);
            g.RotateTransform(angle);

            StringFormat format = new();
            SizeF stringSize = g.MeasureString(text, font, 300 * _gridSize, format);
            PointF corner = new(-stringSize.Width / 2, -stringSize.Height / 2);
            RectangleF rect = new(corner, stringSize);

            Brush textBrush = new SolidBrush(style.color);
            g.DrawString(text, font, textBrush, rect);
            g.ResetTransform();

            format.Dispose();
            font.Dispose();
            textBrush.Dispose();
        }

        private void DrawText(Graphics g, PanelText text)
        {
            Point p = text.GetPoint(_gridSize);
            Transform(ref p);

            DrawText(g, text.Text, p, text.Style, text.Angle);
        }

        private void DrawDetector(Graphics g, TrackDetector detector)
        {
            DrawingContext context = new()
            {
                drawing = this,
                graphics = g
            };

            detector.Style.Draw(context, detector);
        }

        [SupportedOSPlatform("windows")]
        private void DrawGrid(Graphics g)
        {
            if (_gridStyle == null)
                return;

            List<Point> textPoints = [];
            List<string> textStrings = [];

            Brush gridMajorBrush = new SolidBrush(_gridStyle.Value.majorColor);
            Brush gridMinorBrush = new SolidBrush(_gridStyle.Value.minorColor);
            Brush textBrush = new SolidBrush(_gridStyle.Value.textColor);
            Font textFont = new(_gridStyle.Value.textFont, _gridStyle.Value.textSize);

            int numH = _canvas.Height / _gridSize;
            int numV = _canvas.Width / _gridSize;

            for (int i = 0; i < numH; i++)
            {
                Point p1 = new(0, i * _gridSize);
                Point p2 = new(_canvas.Width, i * _gridSize);
                Transform(ref p1);
                Transform(ref p2);
                Pen gridPen = new(gridMinorBrush);
                if (i > 0 && i % 10 == 0)
                {
                    textPoints.Add(p1);
                    textStrings.Add(i.ToString());
                    gridPen = new(gridMajorBrush);
                }
                g.DrawLine(gridPen, p1, p2);
                gridPen.Dispose();
            }

            for (int i = 0; i < numV; i++)
            {
                Point p1 = new(i * _gridSize, 0);
                Point p2 = new(i * _gridSize, _canvas.Height);
                Transform(ref p1);
                Transform(ref p2);
                Pen gridPen = new(gridMinorBrush);
                if (i > 0 && i % 10 == 0)
                {
                    textPoints.Add(p1);
                    textStrings.Add(i.ToString());
                    gridPen = new(gridMajorBrush);
                }
                g.DrawLine(gridPen, p1, p2);
                gridPen.Dispose();
            }

            for (int i = 0; i < textPoints.Count; i++)
            {
                g.DrawString(textStrings[i], textFont, textBrush, textPoints[i]);
            }

            gridMajorBrush.Dispose();
            gridMinorBrush.Dispose();
            textBrush.Dispose();
            textFont.Dispose();
        }

        public void Transform(ref Point p)
        {
            p.X += _canvas.X;
            p.Y += _canvas.Y;
        }

        /// <summary>
        /// Transforms a drawing position (relative to module) to absolute position for drawing.
        /// </summary>
        /// <param name="pos">Drawing position.</param>
        /// <returns>Point representing absolute position on draw panel</returns>
        public Point Transform(DrawingPos pos)
        {
            Point p = new(pos.x, pos.y);
            p.X += _canvas.X;
            p.Y += _canvas.Y;
            return p;
        }

        /// <summary>
        /// Inverse transforms a point to drawing position relative to the drawing origin.
        /// </summary>
        /// <param name="p">Point (absolute pos) to transform.</param>
        /// <returns>DrawPos relative to drawing</returns>
        public DrawingPos InverseTransform(Point p)
        {
            DrawingPos pos = new()
            {
                x = p.X - _canvas.X,
                y = p.Y - _canvas.Y
            };
            return pos;
        }

        public static Vector2 PointToVector(Point p)
        {
            return new Vector2(p.X, p.Y);
        }
        public static Point VectorToPoint(Vector2 v)
        {
            return new Point((int)Math.Round(v.X), (int)Math.Round(v.Y));
        }
        public static float VectorAngle(Vector2 from, Vector2 to)
        {
            return float.RadiansToDegrees(Vector2.Dot(from, to));
        }
        public static float VectorAngle(Vector2 v)
        {
            Vector2 up = new(0f, 1f);
            float angle = VectorAngle(v, up);
            if (v.X < 0f)
                angle = -angle;
            return angle;
        }
    }
}
