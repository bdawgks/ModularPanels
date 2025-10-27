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
            public SignalHead[] setWith;
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

        private class SignalSet(BlockController controller, SignalHead signal, TrackRoute? trackRoute, List<Block> blocks, string indClear, string indOccupied, string indUnset)
        {
            readonly BlockController _controller = controller;
            readonly SignalHead _signal = signal;
            readonly TrackRoute? _trackRoute = trackRoute;
            readonly List<Block> _blocks = blocks;
            readonly string _indicationClear = indClear;
            readonly string _indicationOccupied = indOccupied;
            readonly string _indicationUnset = indUnset;
            readonly HashSet<SignalHead> _setWithSignals = [];

            bool _blockOccupied = false;
            bool _set = false;
            bool _autoUnset = false;
            InputCircuit? _lockCircuit;
            SignalSet? _lockedBy = null;

            public SignalHead Signal { get => _signal; }
            public bool BlockOccupied { get { UpdateBlock(); return _blockOccupied; } }
            public bool IsRouteSet { get => _trackRoute == null || _trackRoute.IsSet; }

            public bool IsSet { get => _set; }

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

                string ind = BlockOccupied ? _indicationOccupied : _indicationClear;
                if (ind == _indicationUnset && _autoUnset)
                    return;

                _set = true;
                SetBlocks(true);
                _lockCircuit?.SetActive(true);

                foreach (var sig in _setWithSignals)
                {
                    SignalSet? sigSet = _controller.FindSignalSet(sig);
                    sigSet?.Set();
                    sigSet?.Lock(this);
                }

                _signal.SetAutoDropIndication(_indicationUnset);
                _signal.SetIndicationFixed(ind);
            }

            public void UnSet(bool forced = false)
            {
                if (!forced && (!_set || !CanUnset()))
                    return;

                _set = false;
                SetBlocks(false);
                _lockCircuit?.SetActive(false);

                foreach (var sig in _setWithSignals)
                {
                    SignalSet? sigSet = _controller.FindSignalSet(sig);
                    sigSet?.Unlock(this);
                }

                _signal.ResetLatch();
                _signal.SetIndicationFixed(_indicationUnset);
            }

            public bool CanSet()
            {
                if (_lockedBy != null)
                    return false;

                if (!IsRouteSet)
                    return false;

                foreach (Block b in _blocks)
                {
                    if (b.InUse)
                        return false;
                }

                foreach (SignalHead sig in _setWithSignals)
                {
                    SignalSet? sigSet = _controller.FindSignalSet(sig);
                    if (sigSet == null)
                        return false;

                    if (sigSet.IsSet)
                        continue;

                    if (!sigSet.CanSet())
                        return false;
                }

                return true;
            }

            public bool CanUnset()
            {
                return _lockedBy == null;
            }

            public void SetAutoUnset()
            {
                _autoUnset = true;
                _signal.StateChangedEvents += (obj, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Indication) && e.Indication == _indicationUnset && IsSet)
                    {
                        UnSet(true);
                    }
                };
            }

            public void AddSetWith(SignalHead head)
            {
                _setWithSignals.Add(head);
            }

            public void SetLockCircuit(InputCircuit lockCircuit)
            {
                _lockCircuit = lockCircuit;
            }

            private void Lock(SignalSet lockedBy)
            {
                _lockedBy = lockedBy;
            }

            private void Unlock(SignalSet lockedBy)
            {
                if (_lockedBy == lockedBy)
                    _lockedBy = null;
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

            SignalSet set = new(this, pars.signal, route, blockList, pars.indicationClear, pars.indicationOccupied, pars.indicationUnset);
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

            foreach (var sig in pars.setWith)
            {
                set.AddSetWith(sig);
            }

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
