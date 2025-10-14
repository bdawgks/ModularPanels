
using ModularPanels.CircuitLib;
using ModularPanels.PanelLib;

namespace ModularPanels
{
    public struct JSON_Layout : IJSONInitializer<Layout>
    {
        public string Name { get; set; }
        public string[] Modules { get; set; }

        public readonly Layout Initialize()
        {
            Layout layout = new(Name, Layout.SignalSpace);

            foreach (string m in Modules)
            {
                if (Module.LoadModule(m, layout, out Module? mod))
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

        static PanelLib.SignalSpace? _sigSpace;

        int _width = 0;
        public int Width
        {
            get => _width;
        }

        public static PanelLib.SignalSpace SignalSpace
        {
            get
            {
                _sigSpace ??= new();
                return _sigSpace;
            }
        }

        public Layout(string name, SignalSpace sigSpace)
        {
            _name = name;
            _sigSpace = sigSpace;
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

            foreach (Module m in _modules)
            {
                CircuitComponent? circuits = m.Components.GetComponent<CircuitComponent>();
                circuits?.UpdateCircuits();
            }

            return drawings;
        }
    }
}
