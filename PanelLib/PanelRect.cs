using ModularPanels.DrawLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.PanelLib
{
    public class PanelRect : DrawTransform, IDrawable, IDrawTransformable
    {
        readonly Color _fillColor = Color.Empty;
        readonly Color _borderColor;
        readonly float _borderSize = 2f;

        readonly DrawingPos _topLeft;
        readonly DrawingPos _bottomRight;

        public PanelRect(Color? fillColor, Color borderColor, float borderSize, GridPos topLeft, GridPos bottomRight)
        {
            if (fillColor != null)
                _fillColor = fillColor.Value;
            _borderColor = borderColor;
            _borderSize = borderSize;
            _topLeft = Grid.Instance.TransformPos(topLeft);
            _bottomRight = Grid.Instance.TransformPos(bottomRight);
        }

        public void Draw(DrawingContext context)
        {
            Point topLeftP = Transform(_topLeft);
            Point bottomRightP = Transform(_bottomRight);

            Rectangle rect = new() 
            { 
                X = topLeftP.X,
                Y = topLeftP.Y,
                Width = bottomRightP.X - topLeftP.X,
                Height = bottomRightP.Y - topLeftP.Y
            };

            if (_fillColor != Color.Empty)
            {
                Brush fillBrush = new SolidBrush(_fillColor);
                context.graphics.FillRectangle(fillBrush, rect);
                fillBrush.Dispose();
            }

            Pen borderPen = new(_borderColor, _borderSize);
            context.graphics.DrawRectangle(borderPen, rect);
            borderPen.Dispose();
        }

        public List<DrawTransform> GetTransforms()
        {
            return [this];
        }
    }
}
