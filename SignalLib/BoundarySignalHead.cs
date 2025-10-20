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

        public BoundarySignalHead(string id, Signal parent, BoundarySignal.BoundaryDir dir) : base(id, parent)
        {
            _dir = dir;
        }

        //internal override SignalHead? PrecedingSignal 
        //{ 
        //    get => _repeatedHeadPrev?.PrecedingSignal;
        //    set 
        //    {
        //        if (_repeatedHeadPrev != null)
        //            _repeatedHeadPrev.PrecedingSignal = value;
        //    }
        //}

        public override string Aspect 
        { 
            get 
            {
                if (_repeatedHeadNext == null)
                {
                    if (_activeRoute != null && _activeRoute.NextSignal != null)
                        return _activeRoute.NextSignal.Aspect;
                    else
                        return string.Empty;
                }

                return _repeatedHeadNext.Aspect; 
            } 
            set { } 
        }

        //internal override SignalHead? AdvancedSignal { get => _repeatedHeadNext?.AdvancedSignal; }

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
