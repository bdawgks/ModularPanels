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
        int _moduleMaxH = 0;
        float _scale = 1f;
        float _leftEdge = 0;

        const int _borderH = 5;
        const int _maxMapH = 50;

        readonly Color _colorHighlight = Color.FloralWhite;
        readonly Color _colorOutside = Color.Gainsboro;
        readonly Color _colorViewBorder = Color.SteelBlue;

        int _panelOffsetY = 0;

        public void Init()
        {
            foreach (Module mod in _layout.Modules)
            {
                _length += Grid.Instance.Scale(mod.Width);
                _moduleMaxH = Math.Max(_moduleMaxH, Grid.Instance.Scale(mod.Height));
            }

            // Set background color
            _mapPanel.BackColor = _colorOutside;

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
            float offsetX = 0;
            float offsetY = 1f;

            float w = _window.Width;
            _scale = w / _length;
            float h = _moduleMaxH * _scale;
            if (h > _maxMapH)
            {
                h = _maxMapH;
                _scale = h / _moduleMaxH;
                w = _length * _scale;
                offsetX = (_mapPanel.Width / 2f) - (w / 2f);
            }
            _leftEdge = offsetX;

            // Adjust panel sizes
            _mapPanel.Size = new() { Height = (int)MathF.Ceiling(h) + 2, Width = _mapPanel.Width };
            _mapPanel.Location = new() { X = _mapPanel.Location.X, Y = _window.Height - _panelOffsetY -  _mapPanel.Height };

            _mainPanel.Size = new() { Width = _mainPanel.Width, Height = _mapPanel.Top - _mainPanel.Top - _borderH };

            Pen borderPen = new(new SolidBrush(Color.Black), 0.5f);
            Pen trackPen = new(new SolidBrush(Color.Black), 2f);

            // Get current view
            float viewOffsetLeft = -_mainPanel.Left;
            float viewLeft = _leftEdge + viewOffsetLeft * _scale;
            float viewWidth = _window.Width * _scale;

            RectangleF viewRect = new(viewLeft, offsetY, viewWidth, h);
            Brush viewBrush = new SolidBrush(_colorHighlight);
            g.FillRectangle(viewBrush, viewRect);

            // Draw track
            foreach (Module mod in _layout.Modules)
            {
                float len = Grid.Instance.Scale(mod.Width) * _scale;
                float height = Grid.Instance.Scale(mod.Height) * _scale;

                RectangleF rect = new(offsetX, offsetY, len, height);

                g.DrawRectangle(borderPen, rect);

                foreach (TrackSegment seg in mod.TrackSegments.Values)
                {
                    PointF p1 = GetGridPos(seg.n0.pos, _scale, offsetX, offsetY);
                    PointF p2 = GetGridPos(seg.n1.pos, _scale, offsetX, offsetY);

                    g.DrawLine(trackPen, p1, p2);
                }

                offsetX += len;
            }

            // Draw view border
            Pen viewPen = new(new SolidBrush(_colorViewBorder), 2f);
            g.DrawRectangle(viewPen, viewRect);

            borderPen.Dispose();
            trackPen.Dispose();
            viewPen.Dispose();
            viewBrush.Dispose();
        }
    }
}
