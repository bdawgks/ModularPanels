
using ModularPanels.SignalLib;
using ModularPanels.TrackLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.CircuitLib
{
    public class RouteCircuit : Circuit
    {
        readonly TrackRoute _route;

        public RouteCircuit(string name, TrackRoute route) : base(name)
        {
            Description = "undescribed route circuit";
            _route = route;
        }

        public override void Init()
        {
            Update();
        }

        private void OnPointsChanged(object? sender, TrackLib.TrackPoints.PointsStateChangeArgs e)
        {
            Update();
        }

        private void Update()
        {
            SetActive(_route.IsSet);
        }
    }
}
