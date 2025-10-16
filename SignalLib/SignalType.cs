using ModularPanels.PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public class SignalType
    {
        readonly string _name;
        readonly SignalRuleset? _ruleset;
        string? _startIndication = null;
        protected readonly HashSet<string> _headNames = [];
        readonly List<SignalShape> _shapes = new();

        public SignalRuleset? Ruleset { get { return _ruleset; } }
        internal List<SignalShape> Shapes { get { return _shapes; } }

        public string? StartIndication
        {
            get { return _startIndication; }
            set { _startIndication = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public SignalType(string name, SignalRuleset? ruleset)
        {
            _name = name;
            _ruleset = ruleset;
        }

        public List<string> GetHeadNames()
        {
            return [.. _headNames];
        }

        public void AddShape(string id, string[] aspect, int[] offset, float angle, string? headId = null, ShapeMirror mirror = ShapeMirror.None)
        {
            Vector2 offsetVector = new();
            if (offset.Length == 2)
            {
                offsetVector.X = offset[0];
                offsetVector.Y = offset[1];
            }

            if (headId != null)
            {
                if (!_headNames.Contains(headId))
                    _headNames.Add(headId);
            }

            _shapes.Add(new SignalShape(id, aspect, offsetVector, angle, headId, mirror));
        }
        public void LoadShapes(ShapeBank bank)
        {
            for (int i = 0; i < _shapes.Count; i++)
            {
                _shapes[i].InitShape(bank);
            }
        }
    }
}
