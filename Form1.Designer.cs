namespace youtube
{
    partial class Form1
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
            txtUrl = new TextBox();
            btnPlay = new Button();
            btnStop = new Button();
            lblStatus = new Label();
            label1 = new Label();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // txtUrl
            // 
            txtUrl.Location = new Point(12, 356);
            txtUrl.Name = "txtUrl";
            txtUrl.PlaceholderText = "Type the URL here";
            txtUrl.Size = new Size(298, 23);
            txtUrl.TabIndex = 0;
            txtUrl.TextChanged += SearchChose;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(338, 356);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(75, 23);
            btnPlay.TabIndex = 1;
            btnPlay.Text = "Play";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Location = new Point(450, 356);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(75, 23);
            btnStop.TabIndex = 2;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Clicky;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 317);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(16, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "   ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Cambria", 24F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(255, 0, 0);
            label1.Location = new Point(246, 9);
            label1.Name = "label1";
            label1.Size = new Size(233, 37);
            label1.TabIndex = 4;
            label1.Text = "Youtube player";
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(12, 68);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(251, 166);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(754, 391);
            Controls.Add(pictureBox1);
            Controls.Add(label1);
            Controls.Add(lblStatus);
            Controls.Add(btnStop);
            Controls.Add(btnPlay);
            Controls.Add(txtUrl);
            MaximumSize = new Size(770, 430);
            MinimumSize = new Size(770, 430);
            Name = "Form1";
            Text = "youtube";
            Load += Form1_Loady;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private TextBox txtUrl;
        private Button btnPlay;
        private Button btnStop;
        private Label lblStatus;
        private Label label1;
        private PictureBox pictureBox1;
    }
}
