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
            title = new Label();
            previewImg = new PictureBox();
            VidTitle = new Label();
            openSearch = new Button();
            ((System.ComponentModel.ISupportInitialize)previewImg).BeginInit();
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
            btnPlay.Location = new Point(330, 355);
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
            btnStop.Location = new Point(420, 356);
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
            // title
            // 
            title.AutoSize = true;
            title.BackColor = Color.Transparent;
            title.Font = new Font("Cambria", 24F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            title.ForeColor = Color.FromArgb(255, 0, 0);
            title.Location = new Point(246, 9);
            title.Name = "title";
            title.Size = new Size(234, 37);
            title.TabIndex = 4;
            title.Text = "Youtube Player";
            // 
            // previewImg
            // 
            previewImg.Location = new Point(12, 68);
            previewImg.Name = "previewImg";
            previewImg.Size = new Size(251, 166);
            previewImg.SizeMode = PictureBoxSizeMode.Zoom;
            previewImg.TabIndex = 5;
            previewImg.TabStop = false;
            // 
            // VidTitle
            // 
            VidTitle.AutoSize = true;
            VidTitle.Font = new Font("Arial", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            VidTitle.Location = new Point(12, 237);
            VidTitle.Name = "VidTitle";
            VidTitle.Size = new Size(25, 22);
            VidTitle.TabIndex = 6;
            VidTitle.Text = "   ";
            // 
            // openSearch
            // 
            openSearch.Enabled = false;
            openSearch.Location = new Point(510, 356);
            openSearch.Name = "openSearch";
            openSearch.Size = new Size(88, 23);
            openSearch.TabIndex = 7;
            openSearch.Text = "Open Search";
            openSearch.UseVisualStyleBackColor = true;
            openSearch.Click += openSearch_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(754, 391);
            Controls.Add(openSearch);
            Controls.Add(VidTitle);
            Controls.Add(previewImg);
            Controls.Add(title);
            Controls.Add(lblStatus);
            Controls.Add(btnStop);
            Controls.Add(btnPlay);
            Controls.Add(txtUrl);
            MaximumSize = new Size(770, 430);
            MinimumSize = new Size(770, 430);
            Name = "Form1";
            Text = "Youtube Player";
            Load += Form1_Loady;
            ((System.ComponentModel.ISupportInitialize)previewImg).EndInit();
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
        private Label title;
        private PictureBox previewImg;
        private Label VidTitle;
        private Button openSearch;
    }
}
