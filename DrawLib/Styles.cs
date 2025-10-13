using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.DrawLib
{
    public class BankSingleton<T>
    {
        static BankSingleton<T>? _instance;

        readonly Dictionary<string, T> _items = [];

        public static G Instance<G>() where G : BankSingleton<T>, new()
        {
            _instance ??= new G();
            return (G)_instance;
        }

        public bool HasItem(string name)
        {
            return _items.ContainsKey(name);
        }

        public void AddItem(string name, T item)
        {
            _items.Add(name, item);
        }

        public bool TryGetItem(string name, out T? item)
        {
            return _items.TryGetValue(name, out item);
        }
    }

    public static class StyleBank
    {
        public static BankSingleton<TrackStyle> TrackStyles
        {
            get
            {
                return BankSingleton<TrackStyle>.Instance<BankSingleton<TrackStyle>>();
            }
        }
        public static BankSingleton<PointsStyle> PointsStyles
        {
            get
            {
                return BankSingleton<PointsStyle>.Instance<BankSingleton<PointsStyle>>();
            }
        }
        public static BankSingleton<DetectorStyle> DetectorStyles
        {
            get
            {
                return BankSingleton<DetectorStyle>.Instance<BankSingleton<DetectorStyle>>();
            }
        }
        public static BankSingleton<TextStyle> TextStyles
        {
            get
            {
                return BankSingleton<TextStyle>.Instance<BankSingleton<TextStyle>>();
            }
        }
    }
}
