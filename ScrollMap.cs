using ModularPanels.DrawLib;
using ModularPanels.TrackLib;

namespace ModularPanels
{
    public class ScrollMap(Layout layout, DrawPanel mapPanel, DrawPanel mainPanel, Control window)
    {
        readonly Layout _layout = layout;
        readonly DrawPanel _mapPanel = mapPanel;
        readonly DrawPanel _mainPanel = mainPanel;
        readonly Control _window = window;

        int _length = 0;
        int _maxH = 0;
        const int _borderH = 5;

        int _panelOffsetY = 0;

        public void Init()
        {
            foreach (Module mod in _layout.Modules)
            {
                _length += Grid.Instance.Scale(mod.Width);
                _maxH = Math.Max(_maxH, Grid.Instance.Scale(mod.Height));
            }

            // TEMPORARY
            _mapPanel.BackColor = Color.Gray;

            _panelOffsetY = _window.Height - _mapPanel.Bottom;
        }

        private static PointF GetGridPos(GridPos gp, float scale, float offsetX, float offsetY)
        {
            DrawingPos dp = gp.ToDrawingPos();
            float x = offsetX + (dp.x * scale);
            float y = offsetY + (dp.y * scale);
            return new(x, y);
        }

        public void Draw(Graphics g)
        {

            float w = _window.Width;
            float scale = w / _length;
            float h = _maxH * scale;
            float offsetX = 0;

            _mapPanel.Size = new() { Height = (int)MathF.Ceiling(h), Width = _mapPanel.Width };
            _mapPanel.Location = new() { X = _mapPanel.Location.X, Y = _window.Height - _panelOffsetY -  _mapPanel.Height };

            _mainPanel.Size = new() { Width = _mainPanel.Width, Height = _mapPanel.Top - _mainPanel.Top - _borderH };

            float offsetY = 0;

            Pen borderPen = new(new SolidBrush(Color.Black), 0.5f);
            Pen trackPen = new(new SolidBrush(Color.Black), 2f);

            foreach (Module mod in _layout.Modules)
            {
                float len = Grid.Instance.Scale(mod.Width) * scale;
                float height = Grid.Instance.Scale(mod.Height) * scale;

                RectangleF rect = new(offsetX, offsetY, len, height);

                g.DrawRectangle(borderPen, rect);

                foreach (TrackSegment seg in mod.TrackSegments.Values)
                {
                    PointF p1 = GetGridPos(seg.n0.pos, scale, offsetX, offsetY);
                    PointF p2 = GetGridPos(seg.n1.pos, scale, offsetX, offsetY);

                    g.DrawLine(trackPen, p1, p2);
                }

                offsetX += len;
            }
        }
    }
}
