using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModularPanels
{
    public partial class CircuitMonitor : Form
    {
        Module? _module = null;

        public CircuitMonitor()
        {
            InitializeComponent();
        }

        public void SetModule(Module module)
        {
            _module = module;
            _module.GetCircuitComponent().CircuitChangeEvents += CircuitMonitor_CircuitChangeEvents;

            UpdateText();
        }

        private void CircuitMonitor_CircuitChangeEvents(object? sender, EventArgs e)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            textBox1.Text = "";
            if (_module == null)
                return;

            foreach (var circuit in _module.GetCircuitComponent().GetCircuits())
            {
                textBox1.Text += string.Format("[{0}] {1} = {2}", circuit.Name, circuit.Description, circuit.Active) + Environment.NewLine;
            }
        }
    }
}
