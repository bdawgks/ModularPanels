using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.DrawLib
{
    public interface IDrawTransformable
    {
        public List<DrawTransform> GetTransforms();
    }

    public abstract class DrawTransform
    {
        PanelLib.Drawing? _drawing;

        public PanelLib.Drawing? Drawing
        {
            get => _drawing;
        }

        public void SetDrawing(PanelLib.Drawing drawing)
        {
            _drawing = drawing;
        }

        protected Point Transform(DrawingPos pos)
        {
            if (_drawing == null)
                return new Point(pos.x, pos.y);

            return _drawing.Transform(pos);
        }

        protected DrawingPos InverseTransform(Point p)
        {
            if (_drawing == null)
                return new DrawingPos()
                {
                    x = p.X, y = p.Y
                };

            return _drawing.InverseTransform(p);
        }
    }
}
