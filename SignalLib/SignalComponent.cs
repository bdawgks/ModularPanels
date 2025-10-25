using ModularPanels.Components;

namespace ModularPanels.SignalLib
{
    public class SignalComponent(IParent parent, SignalBank bank) : Component(parent)
    {
        private readonly Dictionary<string, Signal> _signalMap = [];
        private readonly SignalBank _bank = bank;

        private readonly Dictionary<int, BoundarySignal> _leftBoundarySignals = [];
        private readonly Dictionary<int, BoundarySignal> _rightBoundarySignals = [];

        public Dictionary<string, Signal> SignalMap { get { return _signalMap; } }

        public SignalBank Bank
        {
            get { return _bank; }
        }

        public override string? ToString()
        {
            if (Parent is Module mod)
            {
                return string.Format("Signals:{0}", mod.Name);
            }
            return base.ToString();
        }

        public Signal? CreateSignal(string id, string type)
        {
            Signal? sig = _bank.CreateSignal(this, id, type);

            if (sig != null)
                _signalMap.Add(id, sig);

            return sig;
        }

        public void AddBoundarySignal(BoundarySignal sig)
        {
            _signalMap.Add(sig.Name, sig);
            if (sig.Boundary == BoundarySignal.BoundarySide.Left)
                _leftBoundarySignals.Add(sig.Index, sig);
            else if (sig.Boundary == BoundarySignal.BoundarySide.Right)
                _rightBoundarySignals.Add(sig.Index, sig);
        }

        public SignalHead? GetSignalHead(SignalHeadId id)
        {
            if (!_signalMap.TryGetValue(id.id, out Signal? sig))
                return null;

            if (id.head == null)
                return sig.GetDefaultHead();

            return sig.GetHead(id.head);
        }

        public SignalHead? GetRouteSignalHead(SignalHeadId id, bool nextSignal = false)
        {
            if (!_signalMap.TryGetValue(id.id, out Signal? sig))
                return null;

            if (sig is BoundarySignal bSig && nextSignal)
            {
                if (id.head == null)
                    return null;

                return bSig.GetHead(id.head, BoundarySignal.BoundaryDir.Out);
            }

            if (id.head == null)
                return sig.GetDefaultHead();

            return sig.GetHead(id.head);
        }

        public void InitSignals()
        {
            foreach (Signal s in _signalMap.Values)
            {
                s.InitSignal();
            }
        }

        public void InitPost()
        {
            if (Parent is Module mod)
            {
                foreach (BoundarySignal bSig in _leftBoundarySignals.Values)
                {
                    bSig.Init(mod);
                }
            }
        }

        public Dictionary<int, BoundarySignal> GetBoundarySignals(BoundarySignal.BoundarySide boundary)
        {
            if (boundary == BoundarySignal.BoundarySide.Right)
                return _rightBoundarySignals;
            else
                return _leftBoundarySignals;
        }
    }
}
