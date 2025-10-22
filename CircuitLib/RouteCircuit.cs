
using ModularPanels.SignalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.CircuitLib
{
    public class RouteCircuit : Circuit
    {
        readonly List<PointsRoute> _route = [];

        public RouteCircuit(string name) : base(name)
        {
            Description = "undescribed route circuit";
        }

        public void AddRoute(PointsRoute route)
        {
            _route.Add(route);
            route.points.StateChangeEvents += OnPointsChanged;
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
            foreach (PointsRoute route in _route)
            {
                if (!route.IsSet())
                {
                    SetActive(false);
                    return;
                }
            }

            SetActive(true);
        }
    }
}
