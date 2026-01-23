using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using youtube;

namespace youtube
{
    public partial class Setup
    {
        [STAThread]
        public void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        Button btnSetup = new Button();
        ProgressBar progress = new ProgressBar();
        Label lbl = new Label();

        public MainForm()
        {
            Text = "Setup";
            Width = 400;
            Height = 180;

            btnSetup.Text = "Check and Install Dependencies";
            btnSetup.Dock = DockStyle.Top;
            btnSetup.Height = 40;

            progress.Dock = DockStyle.Top;
            progress.Height = 25;

            lbl.Text = "Idle";
            lbl.Dock = DockStyle.Top;
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            btnSetup.Click += async (_, __) =>
            {
                btnSetup.Enabled = false;
                await EnsureDependencies();
                lbl.Text = "Close this window";
                //btnSetup.Enabled = true;
                btnSetup.Text = "Done 👍";

            };

            Controls.Add(lbl);
            Controls.Add(progress);
            Controls.Add(btnSetup);
        }

        // ================= CORE =================

        async Task EnsureDependencies()
        {
            await InstallYtDlp();

            if (!HasVlc())
            {
                string installer = await DownloadVlcInstaller();
                InstallVlc(installer);
            }
        }

        // ================= yt-dlp =================

        async Task InstallYtDlp()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            if (File.Exists(path)) return;

            lbl.Text = "Đang tải yt-dlp...";
            await DownloadFile(
                "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe",
                path
            );
        }

        // ================= VLC =================

        bool HasVlc()
        {
            return File.Exists(@"C:\Program Files\VideoLAN\VLC\vlc.exe") ||
                   File.Exists(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe");
        }

        async Task<string> DownloadVlcInstaller()
        {
            string installer = Path.Combine(Path.GetTempPath(), "vlc.exe");

            if (!File.Exists(installer))
            {
                lbl.Text = "Đang tải VLC...";
                await DownloadFile(
                    "https://get.videolan.org/vlc/3.0.20/win64/vlc-3.0.20-win64.exe",
                    installer
                );
            }

            return installer;
        }

        void InstallVlc(string installerPath)
        {
            lbl.Text = "Đang cài VLC...";
            progress.Style = ProgressBarStyle.Marquee;

            var psi = new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = "/S",
                UseShellExecute = true
            };

            var p = Process.Start(psi);
            p!.WaitForExit();

            progress.Style = ProgressBarStyle.Blocks;
            progress.Value = 100;
        }

        // ================= Utils =================

        async Task DownloadFile(string url, string output)
        {
            progress.Value = 0;

            using var client = new WebClient();

            client.DownloadProgressChanged += (_, e) =>
            {
                progress.Value = e.ProgressPercentage;
                lbl.Text = $"Đang tải... {e.ProgressPercentage}%";
            };

            await client.DownloadFileTaskAsync(url, output);
        }
    }
}
