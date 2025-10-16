using ModularPanels.Components;
using ModularPanels.DrawLib;

namespace ModularPanels.ButtonLib
{
    public class ClickEventArgs(Point point, DrawingPos pos) : EventArgs
    {
        public Point Point { get; set; } = point;
        public DrawingPos Pos { get; set; } = pos;
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

    public class InteractionComponent : Component
    {
        readonly DrawPanel _panel;
        readonly List<IClickable> _clickables = [];

        IClickable? _lastClicked;

        public InteractionComponent(IParent parent, DrawPanel panel) : base(parent)
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
