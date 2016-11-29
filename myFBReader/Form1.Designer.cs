namespace myFBReader
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.bottomLeftPanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tabsPanel = new System.Windows.Forms.Panel();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.messages = new System.Windows.Forms.TextBox();
            this.readButton = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.bottomLeftPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 678);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1182, 25);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(151, 20);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 19);
            // 
            // bottomPanel
            // 
            this.bottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomPanel.AutoSize = true;
            this.bottomPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.bottomPanel.Controls.Add(this.bottomLeftPanel);
            this.bottomPanel.Controls.Add(this.flowLayoutPanel1);
            this.bottomPanel.Location = new System.Drawing.Point(20, 510);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1150, 105);
            this.bottomPanel.TabIndex = 2;
            // 
            // bottomLeftPanel
            // 
            this.bottomLeftPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomLeftPanel.Controls.Add(this.messages);
            this.bottomLeftPanel.Location = new System.Drawing.Point(3, 3);
            this.bottomLeftPanel.Name = "bottomLeftPanel";
            this.bottomLeftPanel.Size = new System.Drawing.Size(780, 98);
            this.bottomLeftPanel.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.readButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(789, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(341, 94);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // tabsPanel
            // 
            this.tabsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabsPanel.AutoSize = true;
            this.tabsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabsPanel.Controls.Add(this.tabControl2);
            this.tabsPanel.Location = new System.Drawing.Point(12, 12);
            this.tabsPanel.Name = "tabsPanel";
            this.tabsPanel.Size = new System.Drawing.Size(1158, 492);
            this.tabsPanel.TabIndex = 3;
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl2.Location = new System.Drawing.Point(6, 7);
            this.tabControl2.Multiline = true;
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(1132, 475);
            this.tabControl2.TabIndex = 1;
            // 
            // messages
            // 
            this.messages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messages.Location = new System.Drawing.Point(3, 3);
            this.messages.Multiline = true;
            this.messages.Name = "messages";
            this.messages.Size = new System.Drawing.Size(774, 91);
            this.messages.TabIndex = 1;
            // 
            // readButton
            // 
            this.readButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.readButton.AutoSize = true;
            this.readButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.readButton.Location = new System.Drawing.Point(3, 3);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(330, 95);
            this.readButton.TabIndex = 1;
            this.readButton.Text = "Читать";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.readButton_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1182, 703);
            this.Controls.Add(this.tabsPanel);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.bottomLeftPanel.ResumeLayout(false);
            this.bottomLeftPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tabsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel bottomLeftPanel;
        private System.Windows.Forms.Panel tabsPanel;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TextBox messages;
        private System.Windows.Forms.Button readButton;
    }
}

