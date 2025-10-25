using ModularPanels.PanelLib;
using ModularPanels.TrackLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public class SignalRoute
    {
        readonly string _indication;
        readonly SignalHead? _nextSignal;
        readonly TrackRoute _pointsRouting;
        DetectorLatch? _detectorLatch;

        public string Indication { get { return _indication; } }

        public SignalHead? NextSignal { get { return _nextSignal; } }

        public DetectorLatch? DetectorLatch { get { return _detectorLatch; } }

        public SignalRoute(string indication, SignalHead? nextSignal, TrackRoute route)
        {
            _indication = indication;
            _nextSignal = nextSignal;
            _pointsRouting = route;
        }

        public void SetDetectorLatch(DetectorLatch latch)
        {
            _detectorLatch = latch;
        }

        public bool IsRouteSet()
        {
            return _pointsRouting.IsSet;
        }
    }
}
