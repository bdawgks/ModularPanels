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
                Width = 800,
                Height = drawPanel1.Height + 200
            };

            drawing1.Canvas = rect;
            _drawings.Add(drawing1);
            drawPanel1.Paint += DrawPanel1_Paint;

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
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error");
                }
            }

        }

        private void DrawPanel1_Paint(object? sender, PaintEventArgs e)
        {
            foreach (PanelLib.Drawing drawing in _drawings)
            {
                drawing.Draw(e.Graphics);
            }
        }
    }
}
