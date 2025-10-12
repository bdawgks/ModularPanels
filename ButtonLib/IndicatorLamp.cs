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

    public class IndicatorLamp
    {
        readonly Point _pos;
        readonly IndicatorLampTemplate _template;

        bool _lampOn;

        public bool LampOn
        {
            get => _lampOn;
            set => _lampOn = value;
        }

        public IndicatorLamp(Point pos, IndicatorLampTemplate template)
        {
            _pos = pos;
            _template = template;
        }

        public void Draw(Graphics g)
        {
            Color color = _lampOn ? _template.colorOn : _template.colorOff;
            Brush brush = new SolidBrush(color);
            Pen pen = new(Color.Black, _template.border);

            g.TranslateTransform(_pos.X, _pos.Y);
            Rectangle rect = new(-_template.size, -_template.size, _template.size * 2, _template.size * 2);
            g.FillEllipse(brush, rect);
            g.DrawEllipse(pen, rect);
            g.ResetTransform();

            brush.Dispose();
            pen.Dispose();
        }
    }
}
