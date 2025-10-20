using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ModularPanels.SignalLib
{
    public class BoundarySignal : Signal
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

        readonly BoundarySide _boundary;
        readonly int _index = -1;
        readonly Dictionary<string, BoundarySignalHead> _headsOut = [];

        public BoundarySide Boundary { get { return _boundary; } }
        public int Index { get { return _index; } }

        public BoundarySignal(string name, SignalType type, BoundarySide boundary, int index) : base(name, type, true)
        {
            _boundary = boundary;
            _index = index;
        }

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
                        if (otherHeadOut != null && otherHeadOut is BoundarySignalHead otherHeadB && headIn is BoundarySignalHead headB)
                        {
                            otherHeadB.SetRepeatedHeadNext(headB);
                            headB.SetRepeatedHeadPrev(otherHeadB);
                        }
                    }

                    foreach (var headOut in _headsOut.Values)
                    {
                        SignalHead? otherHeadIn = otherSig.GetHead(headOut.ID, BoundaryDir.In);
                        if (otherHeadIn != null && otherHeadIn is BoundarySignalHead otherHeadB && headOut is BoundarySignalHead headB)
                        {
                            headB.SetRepeatedHeadNext(otherHeadB);
                            otherHeadB.SetRepeatedHeadPrev(headB);
                        }

                    }
                }
            }
        }
    }
}
