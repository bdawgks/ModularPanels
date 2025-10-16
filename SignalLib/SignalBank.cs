using ModularPanels.PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public class SignalBank
    {
        readonly ShapeBank _shapeBank = new();
        protected readonly Dictionary<string, SignalType> _types = [];
        readonly Dictionary<string, SignalRuleset> _rulesets = [];

        public ShapeBank ShapeBank
        {
            get => _shapeBank;
        }

        public void AddType(SignalType type)
        {
            _types.Add(type.Name, type);
        }

        public SignalType? this[string name]
        {
            get
            {
                if (_types.TryGetValue(name, out SignalType? value))
                    return value;

                return null;
            }
        }
        public void InitShapes()
        {
            foreach (SignalType t in _types.Values)
            {
                t.LoadShapes(_shapeBank);
            }
        }

        public Signal? CreateSignal(string id, string type)
        {
            if (_types.TryGetValue(type, out SignalType? value))
                return new(id, value);

            return null;
        }

        public SignalRuleset? GetRuleset(string name)
        {
            if (_rulesets.TryGetValue(name, out SignalRuleset? value))
                return value;

            return null;
        }

        public SignalRuleset CreateRuleset(string name)
        {
            if (_rulesets.TryGetValue(name, out SignalRuleset? value))
                return value;

            SignalRuleset newRuleset = new();
            _rulesets.Add(name, newRuleset);
            return newRuleset;
        }
    }
}
