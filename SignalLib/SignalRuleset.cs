using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public class SignalRuleIndication
    {
        readonly Dictionary<string, string> _aspects = [];
        string _defaultAspect = "0";

        public void SetAspect(string? nextAspect, string aspect)
        {
            if (nextAspect == null)
            {
                _defaultAspect = aspect;
                return;
            }

            _aspects[nextAspect] = aspect;
        }

        public string GetAspect(string? nextAspect)
        {
            if (_aspects.Count < 1)
                return _defaultAspect;

            if (nextAspect == null)
                return _defaultAspect;

            if (_aspects.ContainsKey(nextAspect))
                return _aspects[nextAspect];

            return _defaultAspect;
        }
    }

    public class SignalRuleset
    {
        readonly Dictionary<string, SignalRuleIndication> _indications = [];
        readonly List<string> _aspects = [];
        string _defaultIndication = "";

        public string DefaultIndication
        {
            get { return _defaultIndication; }
            set { _defaultIndication = value; }
        }

        public SignalRuleIndication AddIndication(string indicationName)
        {
            if (_indications.ContainsKey(indicationName))
                return _indications[indicationName];

            SignalRuleIndication newIndication = new();
            _indications.Add(indicationName, newIndication);

            if (_defaultIndication == string.Empty)
                _defaultIndication = indicationName;

            return newIndication;
        }

        public string? GetAspect(string indicationName, string? nextAspect)
        {
            if (_indications.TryGetValue(indicationName, out SignalRuleIndication? value))
                return value.GetAspect(nextAspect);

            return null;
        }

        public void SetAspectIndex(string aspect, int idx)
        {
            if (GetAspectIndex(aspect) != -1)
                return;

            _aspects.Insert(idx, aspect);
        }

        public int GetAspectIndex(string aspect)
        {
            return _aspects.FindIndex(s => s.Equals(aspect));
        }

        public string? GetAspectFromIndex(int idx)
        {
            if (idx < 0 || idx >= _aspects.Count)
                return null;

            return _aspects[idx];
        }
    }
}
