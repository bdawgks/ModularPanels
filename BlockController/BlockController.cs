using ModularPanels.CircuitLib;
using ModularPanels.SignalLib;
using ModularPanels.TrackLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.BlockController
{
    public class BlockController
    {
        public struct SignalSetParams
        {
            public SignalHead signal;
            public string? route;
            public string[] blocks;
            public string indicationClear;
            public string indicationOccupied;
            public string indicationUnset;
            public bool autoUnset;
        }

        private class Block(string name)
        {
            readonly string _name = name;
            TrackDetector? _detector;
            bool _inUse;

            public bool InUse {  get { return _inUse; } set { _inUse = value; } }

            public bool IsOccupied { get { return _detector != null && _detector.IsOccupied; } }

            public void SetDetector(TrackDetector detector) { _detector = detector; }

            public override string ToString()
            {
                return _name;
            }
        }

        private class SignalSet(SignalHead signal, TrackRoute? trackRoute, List<Block> blocks, string indClear, string indOccupied, string indUnset)
        {
            readonly SignalHead _signal = signal;
            readonly TrackRoute? _trackRoute = trackRoute;
            readonly List<Block> _blocks = blocks;
            readonly string _indicationClear = indClear;
            readonly string _indicationOccupied = indOccupied;
            readonly string _indicationUnset = indUnset;

            bool _blockOccupied = false;
            bool _set = false;
            InputCircuit? _lockCircuit;

            public SignalHead Signal { get => _signal; }
            public bool BlockOccupied { get { UpdateBlock(); return _blockOccupied; } }
            public bool IsRouteSet { get => _trackRoute == null || _trackRoute.IsSet; }

            private void UpdateBlock()
            {
                foreach (Block b in _blocks)
                {
                    if (b.IsOccupied)
                    {
                        _blockOccupied = true;
                        return;
                    }
                }
                _blockOccupied = false;
            }

            private void SetBlocks(bool set)
            {
                foreach (Block b in _blocks)
                {
                    b.InUse = set;
                }
            }

            public void Set()
            {
                if (_set || !CanSet())
                    return;

                _set = true;
                SetBlocks(true);
                _lockCircuit?.SetActive(true);
                string ind = BlockOccupied ? _indicationOccupied : _indicationClear;
                _signal.SetAutoDropIndication(_indicationUnset);
                _signal.SetIndicationFixed(ind);
            }

            public void UnSet()
            {
                if (!_set)
                    return;

                _set = false;
                SetBlocks(false);
                _lockCircuit?.SetActive(false);
                _signal.ResetLatch();
                _signal.SetIndicationFixed(_indicationUnset);
            }

            public bool CanSet()
            {
                if (!IsRouteSet)
                    return false;

                foreach (Block b in _blocks)
                {
                    if (b.InUse)
                        return false;
                }
                return true;
            }

            public void SetAutoUnset()
            {
                _signal.StateChangedEvents += (obj, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Indication) && e.Indication == _indicationUnset)
                    {
                        UnSet();
                    }
                };
            }

            public void SetLockCircuit(InputCircuit lockCircuit)
            {
                _lockCircuit = lockCircuit;
            }
        }

        readonly Dictionary<string, Block> _blocks = [];
        readonly Dictionary<string, TrackRoute> _routes = [];
        readonly List<SignalSet> _signalSets = [];

        public bool AddRoute(string name, TrackRoute route)
        {
            return _routes.TryAdd(name, route);
        }

        public bool AddBlock(string name, TrackDetector? detector)
        {
            if (_blocks.ContainsKey(name))
                return false;

            Block block = new(name);
            if (detector != null)
                block.SetDetector(detector);
            _blocks.Add(name, block);
            return true;
        }

        public bool AddSignalSet(SignalSetParams pars, Circuit? circuitSet, Circuit? circuitUnset, InputCircuit? circuitLocked)
        {
            TrackRoute? route = null;
            if (!string.IsNullOrEmpty(pars.route))
                _routes.TryGetValue(pars.route, out route);

            List<Block> blockList = [];
            foreach (var block in pars.blocks)
            {
                if (!_blocks.TryGetValue(block, out Block? b))
                    continue;

                blockList.Add(b);
            }

            SignalSet set = new(pars.signal, route, blockList, pars.indicationClear, pars.indicationOccupied, pars.indicationUnset);
            _signalSets.Add(set);

            if (circuitSet != null)
            {
                circuitSet.ActivationEvents += (obj, e) =>
                {
                    if (e.Active)
                        set.Set();
                };
            }

            if (circuitUnset != null)
            {
                circuitUnset.ActivationEvents += (obj, e) =>
                {
                    if (e.Active)
                        set.UnSet();
                };
            }

            if (circuitLocked != null)
                set.SetLockCircuit(circuitLocked);

            if (pars.autoUnset)
                set.SetAutoUnset();

            return true;
        }

        private SignalSet? FindSignalSet(SignalHead head)
        {
            foreach (var ss in _signalSets)
            {
                if (ss.Signal == head && ss.IsRouteSet)
                    return ss;
            }

            return null;
        }

        public bool CanSetSignal(SignalHead head)
        {
            SignalSet? set = FindSignalSet(head);
            if (set == null)
                return false;

            return set.CanSet();
        }

        public bool TrySetSignal(SignalHead head)
        {
            SignalSet? set = FindSignalSet(head);
            if (set == null)
                return false;

            if (!CanSetSignal(head))
                return false;

            set.Set();
            return true;
        }

        public void UnsetSignal(SignalHead head)
        {
            SignalSet? set = FindSignalSet(head);
            if (set == null)
                return;

            set.UnSet();
        }
    }
}
