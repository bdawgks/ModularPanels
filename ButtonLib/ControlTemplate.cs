using System.Diagnostics.CodeAnalysis;

namespace ModularPanels.ButtonLib
{
    public interface ITemplate
    {
        public string Name { get; }
    }

    public class TemplateBank<T> where T : ITemplate
    {
        readonly Dictionary<string, T> _templates = [];

        static TemplateBank<T>? _bank;

        public static TemplateBank<T> Instance
        {
            get
            {
                _bank ??= new TemplateBank<T>();
                return _bank;
            }
        }

        public bool AddItem(T item)
        {
            if (_templates.ContainsKey(item.Name))
                return false;

            _templates.Add(item.Name, item);
            return true;
        }

        public bool TryGetValue(string name, [MaybeNullWhen(false)] out T value)
        {
            return _templates.TryGetValue(name, out value);
        }
    }
}
