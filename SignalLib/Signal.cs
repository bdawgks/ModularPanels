using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public class Signal
    {
        protected readonly string _name;
        protected readonly SignalType _type;
        protected readonly Dictionary<string, SignalHead> _heads = [];
        readonly SignalHead? _defaultHead;
        readonly SignalComponent _component;

        Point _pos = new();
        float _angle = 0.0f;
        float _scale = 1.0f;

        public string Name { get { return _name; } }
        public Point Pos { get { return _pos; } }
        public SignalType Type { get { return _type; } }
        public float Angle { get { return _angle; } }
        public float Scale { get { return _scale; } }

        public Signal(SignalComponent comp, string name, SignalType type)
        {
            _name = name;
            _type = type;
            _component = comp;

            List<string> headNames = type.GetHeadNames();
            if (headNames.Count < 1)
            {
                _defaultHead = new SignalHeadImpl("default", this, true);
            }
            else
            {
                foreach (string headName in headNames)
                {
                    _heads.Add(headName, new SignalHeadImpl(headName, this));
                }
            }
        }

        protected Signal(SignalComponent comp, string name, SignalType type, bool _)
        {
            _name = name;
            _type = type;
            _component = comp;
        }

        public override string ToString()
        {
            return string.Format("({0}){1}", _component.ToString(), _name);
        }

        public void SetPos(int[] pos)
        {
            if (pos.Length == 2)
            {
                _pos = new Point(pos[0], pos[1]);
            }
        }

        public void SetAngle(float angle)
        {
            _angle = angle;
        }

        public void SetScale(float scale)
        {
            _scale = scale;
        }

        public SignalHead? GetHead(string? id)
        {
            if (id == null)
                return _defaultHead;

            if (_heads.TryGetValue(id, out var head))
                return head;

            return null;
        }

        public SignalHead? GetDefaultHead()
        {
            return _defaultHead;
        }

        public virtual void InitSignal()
        {
            foreach (SignalHead head in _heads.Values)
            {
                head.InitSignal();
            }

            _defaultHead?.InitSignal();
        }

        public SignalRuleset? GetRuleset(string? headId)
        {
            return Type.GetRuleset(headId);
        }

        public static bool ParseSignalID(string id, out string name, out string? head)
        {
            int delimIndex = id.IndexOf(":");
            if (delimIndex > 0)
            {
                name = id.Substring(0, delimIndex);
                head = id.Substring(delimIndex + 1);

                return true;
            }

            name = id;
            head = null;

            return false;
        }
    }
}
