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

    public class SignalHead
    {
        readonly Signal _parent;
        readonly string _id;
        SignalHead? _precedingSignal;
        SignalHead? _advancedSignal;
        readonly List<SignalRoute> _routes = [];
        SignalRoute? _activeRoute;
        string? _indication;
        string _aspect = "0";
        SignalLatchIndication? _latchIndication;

        public EventHandler<SignalStateChangeArgs>? StateChangedEvents { get; set; }

        public string ID
        {
            get { return _id; }
        }

        public string Aspect
        {
            get { return _aspect; }
            set { SetAspect(value); }
        }

        public SignalHead? PrecedingSignal
        {
            get { return _precedingSignal; }
            set { _precedingSignal = value; }
        }

        public SignalHead? AdvancedSignal
        {
            get { return _advancedSignal; }
        }

        public SignalHead(string id, Signal parent)
        {
            _id = id;
            _parent = parent;
            _indication = _parent.Type.StartIndication;
            if (parent.Type.Ruleset != null && _indication != null)
            {
                string? startAspect = parent.Type.Ruleset.GetAspect(_indication, null);
                if (startAspect != null)
                    _aspect = startAspect;
            }
        }

        public void AddRoute(SignalRoute route)
        {
            _routes.Add(route);
        }

        public void InitSignal()
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

        public void SetRouteIndication(string indication)
        {
            if (_latchIndication != null && _latchIndication.IsLatched)
                return;

            string? routeIndicaiton = GetRouteIndication();
            if (routeIndicaiton == null)
            {
                SetIndication(indication);
                return;
            }
            SetIndication(routeIndicaiton);
        }

        public void SetIndication(string indication, bool forced = true)
        {
            if (!forced && _latchIndication != null && _latchIndication.IsLatched)
                return;

            if (forced && _latchIndication != null)
                _latchIndication.Reset();

            _indication = indication;
            UpdateIndication();
        }

        public void SetAutoDropIndication(string dropIndication)
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

        private void UpdateIndication()
        {
            if (_indication == null)
                return;

            if (_parent.Type.Ruleset != null)
            {
                string? nextAspect = null;
                if (_activeRoute != null && _activeRoute.NextSignal != null)
                    nextAspect = _activeRoute.NextSignal.Aspect;

                string? newAspect = _parent.Type.Ruleset.GetAspect(_indication, nextAspect);
                SetAspect(newAspect);
            }

            if (_precedingSignal != null)
            {
                _precedingSignal.UpdateIndication();
            }
        }

        public void UpdateRoute()
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
                        _advancedSignal.PrecedingSignal = this;
                    }

                    _activeRoute = r;
                    return;
                }
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
