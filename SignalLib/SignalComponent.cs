using ModularPanels.Components;

namespace ModularPanels.SignalLib
{
    public class SignalComponent(IParent parent, SignalBank bank) : Component(parent)
    {
        private readonly Dictionary<string, Signal> _signalMap = [];
        private readonly SignalBank _bank = bank;

        public Dictionary<string, Signal> SignalMap { get { return _signalMap; } }

        public SignalBank Bank
        {
            get { return _bank; }
        }

        public Signal? CreateSignal(string id, string type)
        {
            Signal? sig = _bank.CreateSignal(id, type);

            if (sig != null)
                _signalMap.Add(id, sig);

            return sig;
        }

        public SignalHead? GetSignalHead(SignalHeadId id)
        {
            if (!_signalMap.TryGetValue(id.id, out Signal? sig))
                return null;

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
    }
}
