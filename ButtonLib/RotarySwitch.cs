using PanelLib;
using System.Drawing;
using System.Numerics;
using static ModularPanels.ButtonLib.RotarySwitchTemplate;

namespace ModularPanels.ButtonLib
{
    public class RotarySwitchTemplate: ITemplate
    {
        public readonly struct PositionTemplate
        {
            public readonly float angle;
            public readonly float size;
            public readonly bool latching;
            public readonly IndicatorLampTemplate? lamp;

            public PositionTemplate(JSON_Control_RotarySwitchPosition template)
            {
                angle = template.Angle;
                size = template.Size;
                latching = template.Latching;
                if (template.Lamp != null)
                    lamp = new(template.Lamp.Value);
            }
        }

        readonly string _name;
        readonly float _size;
        readonly List<PositionTemplate> _positions = [];
        readonly Color _primaryColor;
        readonly Color _secondaryColor;
        readonly int _centerPos = 0;

        public string Name { get => _name; }
        public float Size { get => _size; }
        public PositionTemplate[] Positions { get => [.. _positions]; }
        public Color PrimaryColor { get => _primaryColor; }
        public Color SecondaryColor { get => _secondaryColor; }
        public int CenterPos { get => _centerPos; }

        public RotarySwitchTemplate(JSON_Control_RotarySwitch templateData)
        {
            _name = templateData.Name;
            _centerPos = templateData.CenterPos;
            _size = templateData.Size;
            _primaryColor = templateData.PrimaryColor;
            _secondaryColor = templateData.SecondaryColor;

            foreach (var s in templateData.Positions)
            {
                _positions.Add(new PositionTemplate(s));
            }
        }
    }

    public class RotarySwitchPosition
    {
        readonly int _index;
        readonly RotarySwitch _switch;
        readonly float _angle;
        readonly float _size;
        readonly bool _latching = true;
        readonly CircularVolume _clickVolume;

        readonly IndicatorLamp? _lamp;
        string? _text;

        Circuit? _activatedCircuit;

        public float Angle { get => _angle; }
        public float Size { get => _size; }
        public bool Latching { get => _latching; }
        public CircularVolume ClickVolume { get => _clickVolume; }

        public RotarySwitchPosition(RotarySwitch parent, int index, PositionTemplate template)
        {
            _index = index;
            _switch = parent;
            _angle = template.angle;
            _size = template.size;
            _latching = template.latching;

            Vector2 pSwitch = Drawing.PointToVector(_switch.Pos);
            float totalAngle = (_switch.Angle + _angle) * MathF.PI / 180;
            float totalOffset = _switch.Size + _size;
            Vector2 posDir = new(MathF.Sin(totalAngle), -MathF.Cos(totalAngle));
            Vector2 pos = pSwitch + posDir * totalOffset;

            _clickVolume = new(Drawing.VectorToPoint(pos), _size);
            _clickVolume.MouseDownEvents += MouseDown;
            _clickVolume.MouseUpEvents += MouseUp;

            if (template.lamp != null)
            {
                _lamp = new(Drawing.VectorToPoint(pos), template.lamp.Value);
            }
        }

        private void MouseDown(object? sender, ClickEventArgs e)
        {
            _switch.SetPosition(_index);
        }

        private void MouseUp(object? sender, ClickEventArgs e)
        {
            if (!_latching)
                _switch.ReturnToCenter(_index);
        }

        public void Draw(Graphics g)
        {
            _lamp?.Draw(g);

            /*
            g.TranslateTransform(_clickVolume.Center.X, _clickVolume.Center.Y);
            RectangleF rect = new(-_clickVolume.Radius, -_clickVolume.Radius, _clickVolume.Radius * 2f, _clickVolume.Radius * 2f);
            Pen p = new Pen(Color.Black, 2f);
            g.DrawEllipse(p, rect);
            g.ResetTransform();
            p.Dispose();*/
        }

        public void EnterPostion()
        {
            _activatedCircuit?.SetActive(true);
        }

        public void LeavePosition()
        {
            _activatedCircuit?.SetActive(false);
        }

        public void SetActivatedCircuit(Circuit circuit)
        {
            _activatedCircuit = circuit;
        }

        public void SetLampActivationCircuit(Circuit circuit)
        {
            if (_lamp == null)
                return;

            circuit.ActivationEvents += (sender, e) =>
            {
                _lamp.LampOn = e.Active;
            };
        }
    }

    public class RotarySwitch : IControl
    {
        readonly InteractionSpace _iSpace;

        readonly RotarySwitchTemplate _template;

        readonly float _size;
        readonly Point _pos;
        readonly float _angle = 0f;
        readonly int _centerPos = 0;

        readonly CircularVolume _switchVolume;
        readonly RotarySwitchPosition[] _positions = [];

        int _curPos = 0;

        public RotarySwitchPosition[] Positions
        {
            get => _positions;
        }

        public float Size { get => _size; }
        public Point Pos { get => _pos; }
        public float Angle { get => _angle; }
        public int CenterPos { get => _centerPos; }

        public RotarySwitch(InteractionSpace iSpace, Point pos, RotarySwitchTemplate template)
        {
            _iSpace = iSpace;
            _template = template;
            _pos = pos;
            _size = template.Size;
            _centerPos = template.CenterPos;
            _curPos = _centerPos;
            _switchVolume = new(pos, _size);
            _positions = new RotarySwitchPosition[template.Positions.Length];
            for (int i = 0; i < _positions.Length; i++)
            {
                _positions[i] = new(this, i, template.Positions[i]);
            }

            _iSpace.AddControl(this);
        }

        public IClickable[] GetClickables()
        {
            IClickable[] array = new IClickable[_positions.Length + 1];
            array[0] = _switchVolume;
            for (int i = 0; i < _positions.Length; i++)
            {
                array[i + 1] = _positions[i].ClickVolume;
            }
            return array;
        }

        public void Draw(Graphics g)
        {
            float curPosAngle = _angle + _positions[_curPos].Angle;
            g.TranslateTransform(_pos.X, _pos.Y);
            g.RotateTransform(curPosAngle);
            RectangleF rect = new(-_size, -_size, _size * 2f, _size * 2f);

            float handleWidth = _size * 0.25f;
            float handleLength = _size * 0.9f;
            RectangleF rectHandle = new(-handleWidth, -handleLength, handleWidth * 2f, handleLength * 2f);

            float notchWidth = handleWidth * 0.5f;
            RectangleF rectNotch = new(-notchWidth, -handleLength, notchWidth * 2f, handleLength);

            Brush primaryBrush = new SolidBrush(_template.PrimaryColor);
            Brush secondaryBrush = new SolidBrush(_template.SecondaryColor);
            Brush lineBrush = new SolidBrush(Color.Black);
            Pen linePen = new(lineBrush, 1.5f);
            g.FillEllipse(primaryBrush, rect);
            g.DrawEllipse(linePen, rect);
            g.DrawRectangle(linePen, rectHandle);
            g.FillRectangle(secondaryBrush, rectNotch);
            g.DrawRectangle(linePen, rectNotch);
            g.ResetTransform();

            foreach (RotarySwitchPosition pos in _positions)
            {
                pos.Draw(g);
            }

            primaryBrush.Dispose();
            secondaryBrush.Dispose();
            lineBrush.Dispose();
            linePen.Dispose();
        }

        public void SetActivatedCircuit(int posIndex, string circuitName)
        {
            if (!_iSpace.TryGetCircuit(circuitName, out Circuit? circuit))
                return;

            _positions[posIndex].SetActivatedCircuit(circuit!);
        }

        public void SetLampActivationCircuit(int posIndex, string circuitName)
        {
            if (!_iSpace.TryGetCircuit(circuitName, out Circuit? circuit))
                return;

            _positions[posIndex].SetLampActivationCircuit(circuit!);
        }

        public void SetPosition(int idx)
        {
            int startPos = _curPos;
            _curPos = idx;
            UpdatePosition(startPos);
        }

        public void ReturnToCenter(int from)
        {
            int startPos = _curPos;
            while (_curPos != _centerPos && !_positions[_curPos].Latching)
            {
                int next = -1;
                if (from <= _centerPos)
                {
                    next = 1;
                }
                _curPos += next;
            }
            UpdatePosition(startPos);
        }

        private void UpdatePosition(int startPos)
        {
            if (_curPos == startPos)
                return;

            _positions[startPos].LeavePosition();
            _positions[_curPos].EnterPostion();
        }
    }
}
