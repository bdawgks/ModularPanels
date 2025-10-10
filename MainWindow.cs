using PanelLib.JSONData;
using System.IO;
using System.Text.Json;

namespace ModularPanels
{
    public partial class MainWindow : Form
    {
        readonly List<PanelLib.Drawing> _drawings = [];

        public MainWindow()
        {
            InitializeComponent();

            PanelLib.Drawing drawing1 = new();
            Rectangle rect = new()
            {
                X = drawPanel1.Left + 200,
                Y = drawPanel1.Top,
                Width = 2000,
                Height = drawPanel1.Height + 200
            };

            drawing1.Canvas = rect;
            _drawings.Add(drawing1);
            drawPanel1.Paint += OnPaint;

            string path = "C:\\Users\\Kyle\\source\\repos\\ModularPanels\\data\\modules\\wilnau.json";
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    JSON_Module? moduleData = JsonSerializer.Deserialize<JSON_Module>(json);

                    if (moduleData != null)
                    {
                        Module module = moduleData.Initialize();
                        module.InitDrawing(drawing1);

                        module.TrackDetectors["1-"].IsOccupied = true;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error");
                }
            }

            MouseWheel += OnMouseWheel;
            SizeChanged += OnResizeEnd;

            drawPanel1.SetMinWidth(2000); ;
            drawPanel1.InitScrollbar(hScrollBar1, this, 20);
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
    }
}
