using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModularPanels.Components
{
    public class ComponentContainer(IParent parent)
    {
        readonly IParent _parent = parent;
        readonly Dictionary<Type, Component> _components = [];

        public bool AddComponent(Component component)
        {
            if (HasComponent(component.GetType()))
                return false;

            _components.Add(component.GetType(), component);
            return true;
        }

        public bool CreateComponent<T>(object[] args, [NotNullWhen(true)] out T? component) where T : Component
        {
            component = null;

            if (HasComponent(typeof(T)))
                return false;

            object[] compArgs = new object[args.Length + 1];
            compArgs[0] = _parent;
            for (int i = 0; i < args.Length; i++)
            {
                compArgs[i + 1] = args[i];
            }
            component = (T?)Activator.CreateInstance(typeof(T), compArgs);

            if (component == null)
                return false;

            AddComponent(component);
            return true;
        }

        public bool HasComponent(Type type)
        {
            return _components.ContainsKey(type);
        }

        public CType? GetComponent<CType>() where CType : Component
        {
            _components.TryGetValue(typeof(CType), out var component);

            if (component == null)
                return null;

            if (component.GetType() == typeof(CType))
            {
                return (CType)component;
            }
            return null;
        }

        public bool TryGetComponent<CType>([NotNullWhen(true)] out CType? component) where CType : Component
        {
            component = GetComponent<CType>();
            if (component == null)
                return false;

            return true;
        }
    }
}
