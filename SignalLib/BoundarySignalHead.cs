using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public class BoundarySignalHead : SignalHead
    {
        SignalHead? _repeatedHeadNext;
        SignalHead? _repeatedHeadPrev;
        readonly BoundarySignal.BoundaryDir _dir;
        readonly string _defaultIndication;
        readonly string _defaultAspect;

        public override string Aspect 
        { 
            get 
            {
                if (_repeatedHeadNext == null)
                {
                    if (_activeRoute != null && _activeRoute.NextSignal != null)
                        return _activeRoute.NextSignal.Aspect;
                    else
                        return _defaultAspect;
                }

                return _repeatedHeadNext.Aspect; 
            } 
            set { } 
        }

        public BoundarySignalHead(string id
        , Signal parent
        , BoundarySignal.BoundaryDir dir
        , string defaultIndication)
        : base(id, parent)
        {
            _dir = dir;
            _defaultIndication = defaultIndication;

            SignalRuleset? ruleset = _parent.GetRuleset(_id);
            string? defAspect = ruleset?.GetAspect(_defaultIndication);
            _defaultAspect = defAspect ?? string.Empty;
        }

        internal void SetRepeatedHeadNext(SignalHead? repeatedHead)
        {
            _repeatedHeadNext = repeatedHead;
        }

        internal void SetRepeatedHeadPrev(SignalHead? repeatedHead)
        {
            _repeatedHeadPrev = repeatedHead;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1} [{2}]", _parent.Name, _id, _dir.ToString());
        }

        internal override void UpdateRoute()
        {
            if (_repeatedHeadPrev != null)
            {
                base.UpdateRoute();
            }

            _repeatedHeadNext?.UpdateRoute();
        }
        protected override SignalHead? GetRoutePrecedingHead()
        {
            if (_repeatedHeadPrev == null)
                return null;

            return _repeatedHeadPrev.PrecedingSignal;
        }
    }
}
