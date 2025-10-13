using ModularPanels.DrawLib;
using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.ButtonLib
{
    public interface IControl : DrawLib.IDrawable, IDrawTransformable
    {
        public IClickable[] GetClickables();
    }
}
