using ModularPanels.CircuitLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ModularPanels
{
    public partial class CircuitMonitor : Form
    {
        Module? _module = null;
        readonly Dictionary<string, DataGridViewCheckBoxCell> _stateCellMap = [];

        public CircuitMonitor()
        {
            InitializeComponent();
        }

        public void SetModule(Module module)
        {
            _stateCellMap.Clear();

            _module = module;
            _module.GetCircuitComponent().CircuitChangeEvents += CircuitMonitor_CircuitChangeEvents;

            foreach (var circuit in _module.GetCircuitComponent().GetCircuits())
            {
                DataGridViewRow newRow = new();
                DataGridViewTextBoxCell nameCell = new() 
                {
                    Value = circuit.Name
                };
                DataGridViewTextBoxCell descCell = new() 
                {
                    Value = circuit.Description
                };
                DataGridViewCheckBoxCell stateCell = new() 
                {
                    Value = circuit.Active
                };
            
                newRow.Cells.Add(nameCell);
                newRow.Cells.Add(descCell);
                newRow.Cells.Add(stateCell);
                dataGridView1.Rows.Add(newRow);
                nameCell.ReadOnly = true;
                descCell.ReadOnly = true;
                stateCell.ReadOnly = true;
                _stateCellMap.Add(circuit.Name, stateCell);
            }
        }

        private void CircuitMonitor_CircuitChangeEvents(object? sender, CircuitChangeEventArgs e)
        {
            if (_stateCellMap.TryGetValue(e.Circuit.Name, out DataGridViewCheckBoxCell? cell))
            {
                cell.Value = e.Circuit.Active;
            }
        }
    }
}
