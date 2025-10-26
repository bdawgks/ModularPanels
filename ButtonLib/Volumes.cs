using ModularPanels.DrawLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.ButtonLib
{
    public class CircularVolume(DrawingPos center, float radius) : DrawTransform, IClickable
    {
        readonly DrawingPos _center = center;
        readonly float _radius = radius;

        Func<Point, DrawingPos, bool>? _canClickFunc;

        public event EventHandler<ClickEventArgs>? MouseDownEvents;
        public event EventHandler<ClickEventArgs>? MouseUpEvents;

        public DrawingPos Center { get { return _center; } }
        public float Radius { get { return _radius; } }
        public Func<Point, DrawingPos, bool> CanClickFunc
        {
            set { _canClickFunc = value; }
        }


        public bool PointInVolume(Point p)
        {
            DrawingPos pos = InverseTransform(p);
            Vector2 vP = pos.ToVector2();
            Vector2 vC = _center.ToVector2();
            return Vector2.Distance(vP, vC) <= _radius;
        }

        public void MouseDown(Point p)
        {
            MouseDownEvents?.Invoke(this, new ClickEventArgs(p, InverseTransform(p)));
        }

        public void MouseUp(Point p)
        {
            MouseUpEvents?.Invoke(this, new ClickEventArgs(p, InverseTransform(p)));
        }

        public bool CanClick(Point p)
        {
            if (_canClickFunc == null)
                return true;

            return _canClickFunc.Invoke(p, InverseTransform(p));
        }
    }
}
