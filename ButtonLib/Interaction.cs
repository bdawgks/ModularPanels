using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.ButtonLib
{
    public class ClickEventArgs(Point point) : EventArgs
    {
        public Point Point { get; set; } = point;
    }

    public interface IClickable
    {
        /// <summary>
        /// Return whether given point lies within the clickable volume.
        /// </summary>
        /// <param name="p">Point</param>
        /// <returns>true if point is in volume, false otherwise</returns>
        public bool PointInVolume(Point p);

        /// <summary>
        /// Return whether the click interaction can be currently processed
        /// </summary>
        /// <param name="p">Click position</param>
        /// <returns>true if click can be processed, false otherwise</returns>
        public bool CanClick(Point p);

        /// <summary>
        /// Process a mouse down event
        /// </summary>
        /// <param name="p">Click position</param>
        public void MouseDown(Point p);

        /// <summary>
        /// Process a mouse up event
        /// </summary>
        /// <param name="p">Click position</param>
        public void MouseUp(Point p);
    }

    public class CircularVolume(Point center, float radius) : IClickable
    {
        readonly Point _center = center;
        readonly float _radius = radius;

        Func<Point, bool>? _canClickFunc;

        public event EventHandler<ClickEventArgs>? MouseDownEvents;
        public event EventHandler<ClickEventArgs>? MouseUpEvents;

        public Point Center { get { return _center; } }
        public float Radius { get { return _radius; } }
        public Func<Point, bool> CanClickFunc
        {
            set { _canClickFunc = value; }
        }


        public bool PointInVolume(Point p)
        {
            Vector2 vP = Drawing.PointToVector(p);
            Vector2 vC = Drawing.PointToVector(_center);
            return Vector2.Distance(vP, vC) <= _radius;
        }

        public void MouseDown(Point p)
        {
            MouseDownEvents?.Invoke(this, new ClickEventArgs(p));
        }

        public void MouseUp(Point p)
        {
            MouseUpEvents?.Invoke(this, new ClickEventArgs(p));
        }

        public bool CanClick(Point p)
        {
            if (_canClickFunc == null)
                return true;

            return _canClickFunc.Invoke(p);
        }
    }

    public class InteractionSpace
    {
        readonly DrawPanel _panel;
        readonly List<IClickable> _clickables = [];

        IClickable? _lastClicked;

        public InteractionSpace(DrawPanel panel)
        {
            _panel = panel;

            _panel.MouseDown += OnMouseDown;
            _panel.MouseUp += OnMouseUp;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            foreach (var clickable in _clickables)
            {
                if (clickable.PointInVolume(e.Location) && clickable.CanClick(e.Location))
                {
                    clickable.MouseDown(e.Location);
                    _lastClicked = clickable;
                    _panel.Invalidate();
                }
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (_lastClicked != null)
            {
                _lastClicked.MouseUp(e.Location);
                _lastClicked = null;
                _panel.Invalidate();
            }
        }

        public void AddControl(IControl control)
        {
            _clickables.AddRange(control.GetClickables());
        }
    }
}
