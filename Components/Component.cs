using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.Components
{
    public abstract class Component
    {
        readonly IParent _parent;
        public IParent Parent { get { return _parent; } }

        protected Component(IParent parent)
        {
            _parent = parent;
            _parent.AddComponent(this);
        }
    }
}
