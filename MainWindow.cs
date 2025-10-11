using PanelLib;
using PanelLib.JSONData;
using System.IO;
using System.Text.Json;

namespace ModularPanels
{
    public partial class MainWindow : Form
    {
        List<PanelLib.Drawing> _drawings = [];

        public MainWindow()
        {
            InitializeComponent();

            drawPanel1.Paint += OnPaint;

            MouseWheel += OnMouseWheel;
            SizeChanged += OnResizeEnd;
            drawPanel1.InitScrollbar(hScrollBar1, this, 20);

            LoadLibFiles();
            LoadLayout();
        }

        private void LoadLibFiles()
        {
            string dataPath = Application.StartupPath + "data";
            JSONLib.LoadStyleFiles(dataPath + "\\styles\\");
        }

        private void LoadLayout()
        {

            string path = "C:\\Users\\Kyle\\source\\repos\\ModularPanels\\data\\layouts\\testlayout.json";
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
                        drawPanel1.Width = layout.Width;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error");
                }
            }

            drawPanel1.SetMinWidth(drawPanel1.Width);
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
