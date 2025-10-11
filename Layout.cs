
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels
{
    public struct JSON_Layout : IJSONInitializer<Layout>
    {
        public string Name { get; set; }
        public string[] Modules { get; set; }

        public readonly Layout Initialize()
        {
            Layout layout = new(Name);

            foreach (string m in Modules)
            {
                if (Module.LoadModule(m, out Module? mod))
                {
                    layout.AddModule(mod!);
                }
            }

            return layout;
        }
    }

    public class Layout
    {
        readonly string _name;
        readonly List<Module> _modules = [];

        int _width = 0;
        public int Width
        {
            get => _width;
        }

        public Layout(string name)
        {
            _name = name;
        }

        public void AddModule(Module module)
        {
            _modules.Add(module);
        }

        public List<PanelLib.Drawing> GetDrawings()
        {
            List<PanelLib.Drawing> drawings = [];

            int x = 0;
            foreach (Module m in _modules)
            {
                PanelLib.Drawing drawing = new();

                int drawingWidth = m.Width * drawing.GridSize;
                int drawingHeight = m.Height * drawing.GridSize;
                Rectangle rect = new()
                {
                    X = x,
                    Width = drawingWidth,
                    Y = 0,
                    Height = drawingHeight
                };
                drawing.Canvas = rect;
                x += drawingWidth;
                _width += drawingWidth;

                m.InitDrawing(drawing);
                drawings.Add(drawing);
            }

            return drawings;
        }
    }
}
