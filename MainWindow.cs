using ModularPanels.SignalLib;
using ModularPanels.TrackLib;
using System.Text.Json;

namespace ModularPanels
{
    public partial class MainWindow : Form
    {
        public static MainWindow? Instance;

        static SignalBank? _signalBank;

        List<PanelLib.Drawing> _drawings = [];
        ScrollMap? _map;

        public static SignalBank SignalBank { get { _signalBank??= new SignalBank(); return _signalBank; } }

        public DrawPanel DrawPanel { get => drawPanel1; }

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            drawPanel1.Paint += OnPaint;
            mapPanel.Paint += OnPaintMap;

            MouseWheel += OnMouseWheel;
            SizeChanged += OnResizeEnd;
            drawPanel1.InitScrollbar(hScrollBar1, this, 20);

            LoadLibFiles();
            LoadLayout();
        }

        private static void LoadLibFiles()
        {
            string dataPath = Application.StartupPath + "data";
            JSONLib.LoadStyleFiles(dataPath + "\\styles\\");
            JSONLib.LoadSignalFiles(dataPath + "\\signals\\");
            LoadControlTemplates();
        }

        private static void LoadControlTemplates()
        {
            string dir = Application.StartupPath + "data\\controls\\";
            if (!Directory.Exists(dir))
                return;

            string[] allFiles = Directory.GetFiles(dir);
            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) != ".json")
                    continue;

                ButtonLib.ControlTemplatesLoader.LoadTemplateFile(file);
            }
        }

        private void LoadLayout()
        {

            string path = Application.StartupPath + "data\\layouts\\testlayout.json";
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    JSON_Layout? layoutData = JsonSerializer.Deserialize<JSON_Layout>(json);

                    if (layoutData != null)
                    {
                        Layout layout = layoutData.Value.Initialize();
                        _drawings = layout.GetDrawings();
                        drawPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
                        drawPanel1.Width = layout.Width;

                        foreach (var m in layout.Modules)
                        {
                            ToolStripMenuItem item = new()
                            {
                                Text = m.Name,
                            };
                            item.Click += (obj, e) =>
                            {
                                CircuitMonitor monitor = new();
                                monitor.SetModule(m);
                                monitor.Show();
                            };

                            monitorCircuitsToolStripMenuItem.DropDownItems.Add(item);
                        }
                        _map = new(layout, mapPanel, drawPanel1, this);
                        _map.Init();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error");
                }
            }

            drawPanel1.SetMinWidth(drawPanel1.Width);
        }

        public void AddDetectorDebug(Module mod, TrackDetector det)
        {
            ToolStripMenuItem? parent = null;
            if (detectorsToolStripMenuItem.DropDownItems.ContainsKey(mod.Name))
            {
                int i = detectorsToolStripMenuItem.DropDownItems.IndexOfKey(mod.Name);
                if (i >= 0 && detectorsToolStripMenuItem.DropDownItems[i] is ToolStripMenuItem parentMenu)
                    parent = parentMenu;
            }
            if (parent == null)
            {
                parent = new ToolStripMenuItem() { Text = mod.Name };
                detectorsToolStripMenuItem.DropDownItems.Add(parent);
                parent.Name = mod.Name;
            }

            ToolStripMenuItem item = new()
            {
                Text = det.ID
            };
            item.Click += (obj, e) =>
            {
                item.Checked = !item.Checked;
                det.SetOccupied(item.Checked);
                drawPanel1.Invalidate();
            };

            parent.DropDownItems.Add(item);
        }

        private void OnResizeEnd(object? sender, EventArgs e)
        {
            UpdateDrawComponent();
            drawPanel1.UpdateScrollbar();
        }

        private void OnRedraw(object? sender, EventArgs e)
        {
            UpdateDrawComponent();
            drawPanel1.UpdateScrollbar();
        }

        private void UpdateDrawComponent()
        {
            drawPanel1.Invalidate();
            drawPanel1.UpdateScrollbar();
            mapPanel.Invalidate();
        }

        private void OnMouseWheel(object? sender, MouseEventArgs e)
        {
            drawPanel1.ScrollView(e.Delta);
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            foreach (PanelLib.Drawing drawing in _drawings)
            {
                drawing.Draw(e.Graphics);
            }
        }

        private void OnPaintMap(object? sender, PaintEventArgs e)
        {
            _map?.Draw(e.Graphics);
        }
    }
}
