using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.ButtonLib
{
    public interface IControl
    {
        public IClickable[] GetClickables();

        public void Draw(Graphics g);
    }
}
