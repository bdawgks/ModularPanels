using ModularPanels.DrawLib;
using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.ButtonLib
{
    public struct IndicatorLampTemplate(JSON_Control_Lamp template)
    {
        public readonly Color colorOn = template.ColorOn;
        public readonly Color colorOff = template.ColorOff;
        public readonly int size = template.Size;
        public readonly float border = template.Border;
    }

    public class IndicatorLamp : DrawTransform
    {
        readonly DrawLib.DrawingPos _pos;
        readonly IndicatorLampTemplate _template;

        bool _lampOn;

        public bool LampOn
        {
            get => _lampOn;
            set => _lampOn = value;
        }

        public IndicatorLamp(DrawLib.DrawingPos pos, IndicatorLampTemplate template)
        {
            _pos = pos;
            _template = template;
        }

        public void Draw(DrawingContext context)
        {
            Color color = _lampOn ? _template.colorOn : _template.colorOff;
            Brush brush = new SolidBrush(color);
            Pen pen = new(Color.Black, _template.border);

            Graphics g = context.graphics;
            Point point = context.drawing.Transform(_pos);
            g.TranslateTransform(point.X, point.Y);
            Rectangle rect = new(-_template.size, -_template.size, _template.size * 2, _template.size * 2);
            g.FillEllipse(brush, rect);
            g.DrawEllipse(pen, rect);
            g.ResetTransform();

            brush.Dispose();
            pen.Dispose();
        }
    }
}
