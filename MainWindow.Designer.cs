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
            hScrollBar1 = new HScrollBar();
            drawPanel1 = new DrawPanel();
            monitorCircuitsToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, monitorCircuitsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 3, 0, 3);
            menuStrip1.Size = new Size(914, 30);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // hScrollBar1
            // 
            hScrollBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            hScrollBar1.Location = new Point(10, 557);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new Size(894, 23);
            hScrollBar1.TabIndex = 1;
            // 
            // drawPanel1
            // 
            drawPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            drawPanel1.Location = new Point(10, 36);
            drawPanel1.Margin = new Padding(3, 4, 3, 4);
            drawPanel1.Name = "drawPanel1";
            drawPanel1.Size = new Size(894, 517);
            drawPanel1.TabIndex = 2;
            // 
            // monitorCircuitsToolStripMenuItem
            // 
            monitorCircuitsToolStripMenuItem.Name = "monitorCircuitsToolStripMenuItem";
            monitorCircuitsToolStripMenuItem.Size = new Size(128, 24);
            monitorCircuitsToolStripMenuItem.Text = "Monitor Circuits";
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(914, 600);
            Controls.Add(drawPanel1);
            Controls.Add(hScrollBar1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
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
    }
}
