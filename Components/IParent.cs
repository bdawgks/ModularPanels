using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.Components
{
    public interface IParent
    {
        public ComponentContainer Components { get; }

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

    }
}
