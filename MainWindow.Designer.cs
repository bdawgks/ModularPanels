namespace ModularPanels
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            monitorCircuitsToolStripMenuItem = new ToolStripMenuItem();
            detectorsToolStripMenuItem = new ToolStripMenuItem();
            hScrollBar1 = new HScrollBar();
            drawPanel1 = new DrawPanel();
            mapPanel = new DrawPanel();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, monitorCircuitsToolStripMenuItem, detectorsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // monitorCircuitsToolStripMenuItem
            // 
            monitorCircuitsToolStripMenuItem.Name = "monitorCircuitsToolStripMenuItem";
            monitorCircuitsToolStripMenuItem.Size = new Size(105, 20);
            monitorCircuitsToolStripMenuItem.Text = "Monitor Circuits";
            // 
            // detectorsToolStripMenuItem
            // 
            detectorsToolStripMenuItem.Name = "detectorsToolStripMenuItem";
            detectorsToolStripMenuItem.Size = new Size(69, 20);
            detectorsToolStripMenuItem.Text = "Detectors";
            // 
            // hScrollBar1
            // 
            hScrollBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            hScrollBar1.Location = new Point(9, 418);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new Size(782, 23);
            hScrollBar1.TabIndex = 1;
            // 
            // drawPanel1
            // 
            drawPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            drawPanel1.Location = new Point(9, 27);
            drawPanel1.Name = "drawPanel1";
            drawPanel1.Size = new Size(782, 307);
            drawPanel1.TabIndex = 2;
            // 
            // mapPanel
            // 
            mapPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mapPanel.Location = new Point(9, 340);
            mapPanel.Name = "mapPanel";
            mapPanel.Size = new Size(782, 75);
            mapPanel.TabIndex = 3;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(mapPanel);
            Controls.Add(drawPanel1);
            Controls.Add(hScrollBar1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainWindow";
            Text = "Modules";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private HScrollBar hScrollBar1;
        private DrawPanel drawPanel1;
        private ToolStripMenuItem monitorCircuitsToolStripMenuItem;
        private ToolStripMenuItem detectorsToolStripMenuItem;
        private DrawPanel mapPanel;
    }
}
