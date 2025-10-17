using ModularPanels.PanelLib;
using ModularPanels.TrackLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public struct PointsRoute
    {
        public TrackPoints points;
        public TrackPoints.PointsState state;

        public readonly bool IsSet()
        {
            return state == points.State;
        }
    }

    public class SignalRoute
    {
        readonly string _indication;
        readonly SignalHead? _nextSignal;
        readonly List<PointsRoute> _pointsRouting = [];

        public string Indication { get { return _indication; } }

        public SignalHead? NextSignal { get { return _nextSignal; } }

        public SignalRoute(string indication, SignalHead? nextSignal)
        {
            _indication = indication;
            _nextSignal = nextSignal;
        }

        public void AddPointsRoute(PointsRoute pr)
        {
            _pointsRouting.Add(pr);
        }

        public bool IsRouteSet()
        {
            foreach (PointsRoute pr in _pointsRouting)
            {
                if (!pr.IsSet())
                    return false;
            }

            return true;
        }
    }
}
