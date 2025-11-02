using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public abstract class BoundarySignalHead : SignalHead
    {
        protected readonly string _defaultIndication;
        protected readonly string _defaultAspect;

        public BoundarySignalHead(string id, Signal parent, string defaultIndication) : base(id, parent)
        {
            _defaultIndication = defaultIndication;

            SignalRuleset? ruleset = _parent.GetRuleset(_id);
            string? defAspect = ruleset?.GetAspect(_defaultIndication);
            _defaultAspect = defAspect ?? string.Empty;
        }
    }

    public class BoundarySignalHeadOut : BoundarySignalHead
    {
        BoundarySignalHeadIn? _linkedHead;

        internal override SignalHead? PrecedingSignal
        {
            set { SetPrecedingSignal(value); }
        }

        public override string Aspect
        {
            get
            {
                if (_linkedHead == null)
                {
                    return _defaultAspect;
                }

                return _linkedHead.Aspect;
            }
            set { }
        }

        public SignalHead? Linked
        {
            get => _linkedHead;
        }

        public BoundarySignalHeadOut(string id, Signal parent, string defaultIndication)
        : base(id, parent, defaultIndication) { }

        public override string ToString()
        {
            return string.Format("{0}:{1} [Out]", _parent.ToString(), _id);
        }

        public void SetLinkedSignal(BoundarySignalHeadIn sig)
        {
            _linkedHead = sig;
            sig.UpdateRoute();
            sig.SetPrecedingSignal(_precedingSignal);
        }

        internal override void UpdateRoute()
        {
            if (_linkedHead != null)
            {
                _linkedHead.UpdateRoute();
                _advancedSignal = _linkedHead.AdvancedSignal;
            }
        }

        private void SetPrecedingSignal(SignalHead? head)
        {
            _precedingSignal = head;
            if (_linkedHead == null)
                return;

            _linkedHead.SetPrecedingSignal(head);
        }
    }

    public class BoundarySignalHeadIn : BoundarySignalHead
    {
        public override string Aspect
        {
            get
            {
                UpdateRoute();
                if (_advancedSignal == null)
                {
                    return _defaultAspect;
                }

                return _advancedSignal.Aspect;
            }
            set { }
        }

        public BoundarySignalHeadIn(string id, Signal parent, string defaultIndication)
        : base(id, parent, defaultIndication) { }

        public override string ToString()
        {
            return string.Format("{0}:{1} [In]", _parent.ToString(), _id);
        }

        internal override void UpdateRoute()
        {
            base.UpdateRoute();
        }

        public void SetPrecedingSignal(SignalHead? head)
        {
            _precedingSignal = head;
            if (_advancedSignal == null)
                return;

            _advancedSignal.PrecedingSignal = _precedingSignal;
        }
        protected override SignalHead? GetRoutePrecedingHead()
        {
            return _precedingSignal;
        }
    }
}
