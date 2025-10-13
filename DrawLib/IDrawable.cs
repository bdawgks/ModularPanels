using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.DrawLib
{
    public interface IDrawable
    {
        public void Draw(DrawingContext context);
    }
}
