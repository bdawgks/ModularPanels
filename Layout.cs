
using ModularPanels.CircuitLib;
using ModularPanels.PanelLib;

namespace ModularPanels
{
    public struct JSON_Layout : IJSONInitializer<Layout>
    {
        public string Name { get; set; }
        public string[] Modules { get; set; }

        public ScrollMap.MapStyle? MapStyle { get; set; }

        public readonly Layout Initialize()
        {
            Layout layout = new(Name);

            foreach (string m in Modules)
            {
                if (Module.LoadModule(m, layout, out Module? mod))
                {
                    if (layout.Modules.Count > 0)
                    {
                        mod.LeftModule = layout.Modules.Last();
                        layout.Modules.Last().RightModule = mod;
                    }
                    layout.AddModule(mod);

                    mod.GetSignalComponent().InitPost();
                    mod.GetCircuitComponent().InitModule(mod);
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

        public List<Module> Modules
        {
            get => _modules;
        }

        public Layout(string name)
        {
            _name = name;
        }

        public void AddModule(Module module)
        {
            _modules.Add(module);
        }

        public List<Drawing> GetDrawings()
        {
            List<Drawing> drawings = [];

            int x = 0;
            foreach (Module m in _modules)
            {
                Drawing drawing = new();

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

            foreach (Module m in _modules)
            {
                CircuitComponent? circuits = m.Components.GetComponent<CircuitComponent>();
                circuits?.UpdateCircuits();
            }

            return drawings;
        }
    }
}
