using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.Components
{
    public abstract class Component(IParent parent)
    {
        public IParent Parent { get { return parent; } }
    }
}
