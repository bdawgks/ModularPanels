using ModularPanels.DrawLib;
using ModularPanels.JsonLib;
using ModularPanels.PanelLib;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels.TrackLib
{
    public class TrackStyle
    {
        public Color color = Color.Black;
        public float width = 7f;
    }

    public class PointsStyle
    {
        public Color colorInactive = Color.LightGray;
        public Color colorLock = Color.Blue;
        public float lockLength = 20.0f;
        public float lockWidth = 4.0f;
        public float lockSpace = 3.0f;
        public int length = 35;
    }

    public abstract class DetectorStyle
    {
        public Color colorEmpty = Color.Gray;
        public Color colorOccupied = Color.Orange;
        public Color colorOutline = Color.Black;
        public float outline = 2f;

        public abstract void Draw(DrawingContext context, TrackDetector detector);

        public static DetectorStyle Default
        {
            get {  return new DetectorStyleRectangle(); }
        }
    }

    public class DetectorStyleRectangle : DetectorStyle
    {
        public int minEdgeMargin = 5;
        public int segmentLength = 10;
        public int segmentSpace = 5;
        public int width = 5;

        public override void Draw(DrawingContext context, TrackDetector detector)
        {
            Color fillColor = detector.IsOccupied ? colorOccupied : colorEmpty;
            Brush fillBrush = new SolidBrush(fillColor);
            Brush outlineBrush = new SolidBrush(colorOutline);
            Pen outlinePen = new(outlineBrush, outline);

            foreach (TrackSegment seg in detector.GetSegments())
            {
                DrawingPos nodePos0 = seg.n0.pos.ToDrawingPos();
                DrawingPos nodePos1 = seg.n1.pos.ToDrawingPos();
                Vector2 pos0 = Drawing.PointToVector(context.drawing.Transform(nodePos0));
                Vector2 pos1 = Drawing.PointToVector(context.drawing.Transform(nodePos1));
                Vector2 segDir = Vector2.Normalize(pos1 - pos0);
                float angle = Drawing.VectorAngle(segDir) - 90f;

                float segLenF = segmentLength;
                float segLenH = segLenF * 0.5f;
                float segSpaceF = segmentSpace;

                float trackSegLength = Vector2.Distance(pos0, pos1);
                float totalLen = trackSegLength - 2f * minEdgeMargin;
                int numSegs = Math.Max((int)Math.Floor((totalLen - segSpaceF) / (segLenF + segSpaceF)), 1);
                float totalSegLen = segLenF * numSegs;
                float totalSpace = (numSegs - 1) * segSpaceF;
                float remainder = totalLen - totalSegLen - totalSpace;
                float margin = minEdgeMargin + remainder * 0.5f;

                for (int i = 0; i < numSegs; i++)
                {
                    float offset = margin + (i * segLenF) + segLenH + segSpaceF * i;
                    Vector2 pos = pos0 + segDir * offset;
                    context.graphics.TranslateTransform(pos.X, pos.Y);
                    context.graphics.RotateTransform(angle);

                    PointF corner = new(-segLenH, -width * 0.5f);
                    RectangleF rect = new(corner, new SizeF(segLenF, width));
                    context.graphics.FillRectangle(fillBrush, rect);
                    context.graphics.DrawRectangles(outlinePen, [rect]);

                    context.graphics.ResetTransform();
                }
            }

            fillBrush.Dispose();
            outlineBrush.Dispose();
            outlinePen.Dispose();
        }
    }
}
