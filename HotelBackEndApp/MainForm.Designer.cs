namespace HotelBackEndApp
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            start_btn = new Button();
            stop_btn = new Button();
            exe_btn = new Button();
            exit_btn = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripProgressBar1 = new ToolStripProgressBar();
            toolStripSplitButton1 = new ToolStripSplitButton();
            datePickerRange1 = new AntdUI.DatePickerRange();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // start_btn
            // 
            start_btn.Image = Properties.Resources.start;
            start_btn.ImageAlign = ContentAlignment.MiddleLeft;
            start_btn.Location = new Point(633, 41);
            start_btn.Name = "start_btn";
            start_btn.Padding = new Padding(12, 0, 0, 0);
            start_btn.Size = new Size(161, 51);
            start_btn.TabIndex = 0;
            start_btn.Text = "Start";
            start_btn.UseVisualStyleBackColor = true;
            start_btn.Click += start_btn_Click;
            // 
            // stop_btn
            // 
            stop_btn.Image = (Image)resources.GetObject("stop_btn.Image");
            stop_btn.ImageAlign = ContentAlignment.MiddleLeft;
            stop_btn.Location = new Point(633, 138);
            stop_btn.Name = "stop_btn";
            stop_btn.Padding = new Padding(12, 0, 0, 0);
            stop_btn.Size = new Size(161, 51);
            stop_btn.TabIndex = 1;
            stop_btn.Text = "Stop";
            stop_btn.UseVisualStyleBackColor = true;
            stop_btn.Click += stop_btn_Click;
            // 
            // exe_btn
            // 
            exe_btn.Image = Properties.Resources.execute;
            exe_btn.ImageAlign = ContentAlignment.MiddleLeft;
            exe_btn.Location = new Point(633, 235);
            exe_btn.Name = "exe_btn";
            exe_btn.Padding = new Padding(12, 0, 0, 0);
            exe_btn.Size = new Size(161, 51);
            exe_btn.TabIndex = 2;
            exe_btn.Text = "Excute";
            exe_btn.UseVisualStyleBackColor = true;
            exe_btn.Click += exe_btn_Click;
            // 
            // exit_btn
            // 
            exit_btn.Image = (Image)resources.GetObject("exit_btn.Image");
            exit_btn.ImageAlign = ContentAlignment.MiddleLeft;
            exit_btn.Location = new Point(633, 332);
            exit_btn.Name = "exit_btn";
            exit_btn.Padding = new Padding(12, 0, 0, 0);
            exit_btn.Size = new Size(161, 51);
            exit_btn.TabIndex = 3;
            exit_btn.Text = "Exit";
            exit_btn.UseVisualStyleBackColor = true;
            exit_btn.Click += exit_btn_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolStripProgressBar1, toolStripSplitButton1 });
            statusStrip1.Location = new Point(0, 428);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(819, 22);
            statusStrip1.TabIndex = 5;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(100, 17);
            toolStripStatusLabel1.Text = "System Running";
            toolStripStatusLabel1.Click += toolStripStatusLabel1_Click;
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(100, 16);
            toolStripProgressBar1.Step = 5;
            toolStripProgressBar1.Value = 100;
            toolStripProgressBar1.Click += toolStripProgressBar1_Click;
            // 
            // toolStripSplitButton1
            // 
            toolStripSplitButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripSplitButton1.Image = (Image)resources.GetObject("toolStripSplitButton1.Image");
            toolStripSplitButton1.ImageTransparentColor = Color.Magenta;
            toolStripSplitButton1.Name = "toolStripSplitButton1";
            toolStripSplitButton1.Size = new Size(32, 20);
            toolStripSplitButton1.Text = "toolStripSplitButton1";
            // 
            // datePickerRange1
            // 
            datePickerRange1.Font = new Font("Microsoft YaHei UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 134);
            datePickerRange1.Location = new Point(24, 30);
            datePickerRange1.Name = "datePickerRange1";
            datePickerRange1.Size = new Size(469, 62);
            datePickerRange1.TabIndex = 6;
            datePickerRange1.TextAlign = HorizontalAlignment.Center;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(819, 450);
            Controls.Add(datePickerRange1);
            Controls.Add(statusStrip1);
            Controls.Add(exit_btn);
            Controls.Add(exe_btn);
            Controls.Add(stop_btn);
            Controls.Add(start_btn);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "HotelBackEnd";
            Load += MainForm_Load;
            Shown += MainForm_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button start_btn;
        private Button stop_btn;
        private Button exe_btn;
        private Button exit_btn;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripProgressBar toolStripProgressBar1;
        private AntdUI.DatePickerRange datePickerRange1;
        private ToolStripSplitButton toolStripSplitButton1;
    }
}