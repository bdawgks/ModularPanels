using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.Components
{
    public interface IParent
    {
        public ComponentContainer Components { get; }

        public bool CreateComponent<T>(object[] args, [NotNullWhen(true)] out T? component) where T : Component
        {
            return Components.CreateComponent<T>(args, out component);
        }

        public bool AddComponent(Component component)
        {
            return Components.AddComponent(component);
        }

        public bool HasComponent(Type type)
        {
            return Components.HasComponent(type);
        }

        public CType? GetComponent<CType>() where CType : Component
        {
            return Components.GetComponent<CType>();
        }

        public bool TryGetComponent<CType>([NotNullWhen(true)] out CType? component) where CType: Component
        {
            return Components.TryGetComponent(out component);
        }
    }
}
