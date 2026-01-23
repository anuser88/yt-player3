using System.Diagnostics;

namespace youtube
{
    public partial class Form1 : Form
    {
        Process? vlcProcess;

        public Form1()
        {
            InitializeComponent();
        }

        (string video, string audio) GetStreamUrls(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = "-f bv*+ba/b -g " + url,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            string output = p!.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var lines = output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length >= 2)
                return (lines[0], lines[1]);

            // fallback
            return (lines[0], "");
        }


        private void btnPlay_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            }

            lblStatus.Text = "Getting stream URL...";
            //btnPlay.Enabled = false;

            Task.Run(() =>
            {
                string streamUrl = GetStreamUrl(url);

                Invoke(() =>
                {
                    if (string.IsNullOrEmpty(streamUrl))
                    {
                        lblStatus.Text = "Stream not found 💀";
                        btnPlay.Enabled = true;
                        return;
                    }
                    var (video, audio) = GetStreamUrls(url);
                    StartVlc(video,audio);
                });
            });
        }

        private void btnStop_Clicky(object sender, EventArgs e)
        {
            MessageBox.Show("Stop it yourself ;)");
            StopVlc();
        }

        string GetStreamUrl(string url)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"-f bv*+ba/b -g {url}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = Process.Start(psi);
                string output = p!.StandardOutput.ReadToEnd();
                p.WaitForExit();

                // yt-dlp returns multiple lines
                string[] lines = output
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                return lines.Length > 0 ? lines[0] : "";
            }
            catch
            {
                return "";
            }
        }

        void StartVlc(string videoUrl, string audioUrl)
        {
            string vlcPath =
                File.Exists(@"C:\Program Files\VideoLAN\VLC\vlc.exe")
                    ? @"C:\Program Files\VideoLAN\VLC\vlc.exe"
                    : @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";

            string args = $"\"{videoUrl}\"";

            if (!string.IsNullOrEmpty(audioUrl))
                args += $" --input-slave=\"{audioUrl}\"";
            args += " --network-caching=1500 --quiet --no-video-title-show --no-media-library --ignore-config --loop -f \"bv*[vcodec=h264][height<=720]+ba\"";
            lblStatus.Text = "Playing video...";
            Process.Start(new ProcessStartInfo
            {
                FileName = vlcPath,
                Arguments = args,
                UseShellExecute = false
            });
        }


        void StopVlc()
        {
            try
            {
                if (vlcProcess != null && !vlcProcess.HasExited)
                {
                    vlcProcess.Kill(true);
                    vlcProcess.Dispose();
                }
            }
            catch { }

            lblStatus.Text = "Stopped";
            btnPlay.Enabled = true;
        }
    }
}
