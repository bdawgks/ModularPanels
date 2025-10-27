using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ModularPanels.SignalLib
{
    public class BoundarySignal(SignalComponent comp, string name, SignalType type, BoundarySignal.BoundarySide boundary, int index) : Signal(comp, name, type, true)
    {
        public enum BoundarySide
        {
            Left,
            Right
        }
        public enum BoundaryDir
        {
            In,
            Out
        }

        readonly BoundarySide _boundary = boundary;
        readonly int _index = index;
        readonly Dictionary<string, BoundarySignalHead> _headsOut = [];

        public BoundarySide Boundary { get { return _boundary; } }
        public int Index { get { return _index; } }

        internal void AddHead(BoundaryDir dir, BoundarySignalHead head)
        {
            if (dir == BoundaryDir.In)
                _heads.Add(head.ID, head);
            else
                _headsOut.Add(head.ID, head);
        }

        internal SignalHead? GetHead(string name, BoundaryDir dir)
        {
            if (dir == BoundaryDir.In)
                return GetHead(name);

            if (_headsOut.TryGetValue(name, out var head))
                return head;

            return null;
        }

        public override SignalHead? GetHead(SignalHeadId id)
        {
            if (id.boundaryDir == null || id.head == null)
                return null;

            return GetHead(id.head, id.boundaryDir.Value);
        }

        public override void InitSignal()
        {
            foreach (var head in _headsOut.Values)
            {
                head.InitSignal();
            }

            base.InitSignal();
        }

        internal void Init(Module mod)
        {
            if (mod.LeftModule != null)
            {
                var sigMap = mod.LeftModule.GetSignalComponent().GetBoundarySignals(BoundarySide.Right);
                if (sigMap.TryGetValue(_index, out BoundarySignal? otherSig))
                {
                    foreach (var headIn in _heads.Values)
                    {
                        SignalHead? otherHeadOut = otherSig.GetHead(headIn.ID, BoundaryDir.Out);
                        if (otherHeadOut != null && otherHeadOut is BoundarySignalHeadOut otherHeadB && headIn is BoundarySignalHeadIn headB)
                        {
                            otherHeadB.SetLinkedSignal(headB);
                        }
                    }

                    foreach (var headOut in _headsOut.Values)
                    {
                        SignalHead? otherHeadIn = otherSig.GetHead(headOut.ID, BoundaryDir.In);
                        if (otherHeadIn != null && otherHeadIn is BoundarySignalHeadIn otherHeadB && headOut is BoundarySignalHeadOut headB)
                        {
                            headB.SetLinkedSignal(otherHeadB);
                        }
                    }

                    otherSig.InitSignal();
                }
            }
        }
    }
}
