using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public class SignalStateChangeArgs(string aspect, string? indication) : EventArgs
    {
        public string Aspect { get; } = aspect;
        public string? Indication { get; } = indication;
    }

    public abstract class SignalHead(string id, Signal parent)
    {
        protected readonly Signal _parent = parent;
        protected readonly string _id = id;
        protected readonly List<SignalRoute> _routes = [];
        protected SignalRoute? _activeRoute;
        protected SignalHead? _precedingSignal;
        protected SignalHead? _advancedSignal;

        public EventHandler<SignalStateChangeArgs>? StateChangedEvents { get; set; }

        public string ID { get { return _id; } }

        public virtual string Aspect { get { return string.Empty; } set { } }

        internal virtual SignalHead? PrecedingSignal
        {
            get { return _precedingSignal; }
            set { _precedingSignal = value; }
        }

        internal virtual SignalHead? AdvancedSignal
        {
            get { return _advancedSignal; }
        }

        internal virtual void UpdateIndication() { }
        internal virtual void InitSignal() { }

        public virtual void ResetLatch() { }
        public virtual void SetIndicationFixed(string _) { }
        public virtual void SetIndicationLatched(string _) { }
        public virtual void SetRouteIndication(string _) { }
        public virtual void SetAutoDropIndication(string _) { }

        public void AddRoute(SignalRoute route)
        {
            _routes.Add(route);
        }

        internal virtual void UpdateRoute()
        {
            if (_routes.Count < 1)
                return;

            foreach (SignalRoute r in _routes)
            {
                if (r.IsRouteSet())
                {
                    _advancedSignal = r.NextSignal;
                    if (_advancedSignal != null)
                    {
                        _advancedSignal.UpdateRoute();
                        _advancedSignal.PrecedingSignal = GetRoutePrecedingHead();
                    }

                    _activeRoute = r;
                    return;
                }
            }
        }

        protected virtual SignalHead? GetRoutePrecedingHead()
        {
            return this;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", _parent.Name, _id);
        }
    }

    public class SignalHeadImpl : SignalHead
    {
        string? _indication;
        string _aspect = "0";
        SignalLatchIndication? _latchIndication;
        readonly SignalRuleset? _ruleset;

        public override string Aspect
        {
            get { return _aspect; }
            set { SetAspect(value); }
        }

        public SignalHeadImpl(string id, Signal parent, bool isDefault = false) : base(id, parent)
        {
            _ruleset = parent.GetRuleset(isDefault ? null : _id);
            _indication = _parent.Type.StartIndication;
            if (_ruleset != null)
            {
                _indication ??= _ruleset.DefaultIndication;

                string? startAspect = _ruleset.GetAspect(_indication, null);
                if (startAspect != null)
                    _aspect = startAspect;
            }
        }

        internal override void InitSignal()
        {
            UpdateRoute();
        }

        public string? GetRouteIndication()
        {
            UpdateRoute();
            if (_activeRoute != null)
            {
                return _activeRoute.Indication;
            }

            return null;
        }

        public override void SetRouteIndication(string indication)
        {
            if (_latchIndication != null && _latchIndication.IsLatched)
                return;

            string? routeIndicaiton = GetRouteIndication();
            if (routeIndicaiton == null)
            {
                SetIndicationFixed(indication);
                return;
            }
            SetIndicationFixed(routeIndicaiton);
        }

        public override void SetIndicationLatched(string indication)
        {
            if (_latchIndication != null && _latchIndication.IsLatched)
                return;
        }

        public override void SetIndicationFixed(string indication)
        {
            _indication = indication;
            UpdateIndication();
        }

        public override void SetAutoDropIndication(string dropIndication)
        {
            UpdateRoute();
            if (_activeRoute == null)
                return;

            if (_activeRoute.DetectorLatch == null)
                return;

            if (_latchIndication != null && _latchIndication.IsLatched)
                return;

            _activeRoute.DetectorLatch.Set();
            _latchIndication = new(this, dropIndication);
            _latchIndication.SetDetectorLatch(_activeRoute.DetectorLatch);
        }

        public override void ResetLatch()
        {
            if (_latchIndication != null && _latchIndication.IsLatched)
                _latchIndication.Unset();
        }

        internal override void UpdateIndication()
        {
            if (_indication == null)
                return;

            if (_ruleset != null)
            {
                string? nextAspect = null;
                if (_activeRoute != null && _activeRoute.NextSignal != null)
                    nextAspect = _activeRoute.NextSignal.Aspect;

                string? newAspect = _ruleset.GetAspect(_indication, nextAspect);
                SetAspect(newAspect);
            }

            if (_precedingSignal != null)
            {
                _precedingSignal.UpdateIndication();
            }
        }

        private void SetAspect(string? newAspect)
        {
            if (newAspect == null)
                return;

            if (_aspect == newAspect)
                return;

            _aspect = newAspect;
            StateChangedEvents?.Invoke(this, new SignalStateChangeArgs(newAspect, _indication));
        }
    }
}
