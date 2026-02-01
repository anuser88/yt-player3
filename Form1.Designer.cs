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
            avatar = new PictureBox();
            Views = new Label();
            ((System.ComponentModel.ISupportInitialize)previewImg).BeginInit();
            ((System.ComponentModel.ISupportInitialize)avatar).BeginInit();
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
            btnPlay.Click += BtnPlay_Click;
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
            btnStop.Click += BtnStop_Click;
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
            previewImg.Location = new Point(246, 49);
            previewImg.Name = "previewImg";
            previewImg.Size = new Size(240, 160);
            previewImg.SizeMode = PictureBoxSizeMode.Zoom;
            previewImg.TabIndex = 5;
            previewImg.TabStop = false;
            // 
            // VidTitle
            // 
            VidTitle.AllowDrop = true;
            VidTitle.Anchor = AnchorStyles.None;
            VidTitle.AutoEllipsis = true;
            VidTitle.Font = new Font("Arial Narrow", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            VidTitle.Location = new Point(12, 212);
            VidTitle.Name = "VidTitle";
            VidTitle.Size = new Size(730, 22);
            VidTitle.TabIndex = 6;
            VidTitle.Text = "               ";
            VidTitle.TextAlign = ContentAlignment.MiddleCenter;
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
            openSearch.Click += OpenSearch_Click;
            // 
            // avatar
            // 
            avatar.Location = new Point(246, 265);
            avatar.Name = "avatar";
            avatar.Size = new Size(45, 45);
            avatar.SizeMode = PictureBoxSizeMode.Zoom;
            avatar.TabIndex = 8;
            avatar.TabStop = false;
            // 
            // Views
            // 
            Views.Location = new Point(330, 234);
            Views.Name = "Views";
            Views.RightToLeft = RightToLeft.No;
            Views.Size = new Size(156, 16);
            Views.TabIndex = 9;
            Views.Text = "     ";
            Views.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(754, 391);
            Controls.Add(Views);
            Controls.Add(avatar);
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
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)previewImg).EndInit();
            ((System.ComponentModel.ISupportInitialize)avatar).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
        private PictureBox avatar;
        private Label Views;
    }
}
