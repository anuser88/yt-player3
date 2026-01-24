using System.Diagnostics;
using System;
using System.Drawing;
using System.Threading;

namespace youtube
{
    public class ColorUtil
    {
        // h: 0–360, s: 0–1, v: 0–1, a: 0–255
        public static Color HsvToArgb(float h, float s, float v, int a = 255)
        {
            h = h % 360;
            if (h < 0) h += 360;

            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
            float m = v - c;

            float r = 0, g = 0, b = 0;

            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            byte R = (byte)((r + m) * 255);
            byte G = (byte)((g + m) * 255);
            byte B = (byte)((b + m) * 255);

            return Color.FromArgb(a, R, G, B);
        }
    }

    public partial class Form1 : Form
    {
        Process? vlcProcess;

        public Form1()
        {
            InitializeComponent();
        }
        private async void Form1_Loady(object sender, EventArgs e)
        {
            while (true)
            {
                for (int i = 0; i < 180; i++)
                {
                    label1.ForeColor = ColorUtil.HsvToArgb(i*2, 1, 1);
                    await Task.Delay(30);
                }
            }
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
            btnPlay.Enabled = false;

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
            //MessageBox.Show("Stop it yourself ;)");
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
            btnPlay.Enabled = false;
            btnStop.Enabled = true;
            if (!string.IsNullOrEmpty(audioUrl))
                args += $" --input-slave=\"{audioUrl}\"";
            args += " --network-caching=1500 --quiet --no-video-title-show --no-media-library --no-qt-privacy-ask --no-interact --no-qt-name-in-title --ignore-config --loop -f \"bv*[vcodec=h264][height<=720]+ba\"";
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
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/IM vlc.exe /F",
                UseShellExecute = false
            });

            lblStatus.Text = "Stopping...";
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
        }
    }
}
