using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.Components
{
    public class ComponentContainer
    {
        readonly Dictionary<Type, Component> _components = [];

        public bool AddComponent(Component component)
        {
            if (HasComponent(component.GetType()))
                return false;

            _components.Add(component.GetType(), component);
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
    }
}
