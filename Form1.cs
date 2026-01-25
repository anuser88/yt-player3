using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace youtube
{
    public partial class Form1 : Form
    {
        // cache VLC path lookup to avoid repeated IO checks
        private readonly Lazy<string?> _vlcPath = new(() =>
        {
            const string p1 = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            const string p2 = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";
            if (File.Exists(p1)) return p1;
            if (File.Exists(p2)) return p2;
            return null;
        });

        public Form1()
        {
            InitializeComponent();
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TxtUrlText
        {
            get => txtUrl.Text;
            set => txtUrl.Text = value;
        }
        // color animation - keeps UI responsive since it awaits Task.Delay
        private async void Form1_Loady(object sender, EventArgs e)
        {
            var search = new Search();search.Show();
            while (!IsDisposed)
            {
                for (int i = 0; i < 180 && !IsDisposed; i++)
                {
                    label1.ForeColor = HsvToArgb(i * 2, 1, 1);
                    await Task.Delay(30).ConfigureAwait(true);
                }
            }
        }

        // async, non-blocking, and can be cancelled via token
        private async Task<(string video, string audio)> GetStreamUrlsAsync(string url, CancellationToken ct = default)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-f bv*+ba/b -g \"{url}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                return (string.Empty, string.Empty);

            try
            {
                // Read output asynchronously and wait for exit. If cancelled, kill process.
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var exitTask = process.WaitForExitAsync(ct);

                // Wait for both tasks; if token is cancelled WaitForExitAsync will throw OperationCanceledException
                await Task.WhenAll(outputTask, exitTask).ConfigureAwait(false);

                string output = outputTask.Result ?? string.Empty;
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length >= 2) return (lines[0], lines[1]);
                if (lines.Length == 1) return (lines[0], string.Empty);
                return (string.Empty, string.Empty);
            }
            catch (OperationCanceledException)
            {
                try { if (!process.HasExited) process.Kill(true); } catch { }
                throw;
            }
            catch
            {
                try { if (!process.HasExited) process.Kill(true); } catch { }
                throw;
            }
        }

        private async void btnPlay_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
                url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

            lblStatus.Text = "Getting stream URL...";
            btnPlay.Enabled = false;

            try
            {
                // call async GetStreamUrlsAsync so heavy work doesn't run on UI thread
                (string video, string audio) stream;
                try
                {
                    // Run the blocking yt-dlp process off the UI thread
                    stream = await Task.Run(() => GetStreamUrlsAsync(url));
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error getting stream: " + ex.Message;
                    btnPlay.Enabled = true;
                    return;
                }

                var (video, audio) = stream;

                if (string.IsNullOrEmpty(video))
                {
                    lblStatus.Text = "Stream not found 💀";
                    return;
                }

                StartVlc(video, audio);
            }
            catch (Exception ex)
            {
                // keep message short but useful
                lblStatus.Text = "Error getting stream: " + ex.Message;
            }
            finally
            {
                // if VLC started successfully StartVlc disables/enables buttons itself
                if (lblStatus.Text.StartsWith("Error") || lblStatus.Text.Contains("Stream not found"))
                    btnPlay.Enabled = true;
            }
        }

        private void btnStop_Clicky(object sender, EventArgs e)
        {
            StopVlc();
        }

        void StartVlc(string videoUrl, string audioUrl)
        {
            var vlc = _vlcPath.Value;
            if (string.IsNullOrEmpty(vlc))
            {
                lblStatus.Text = "VLC not found";
                btnPlay.Enabled = true;
                return;
            }

            string args = $"\"{videoUrl}\"";
            if (!string.IsNullOrEmpty(audioUrl))
                args += $" --input-slave=\"{audioUrl}\"";

            args += " --network-caching=1500 --quiet --no-video-title-show --no-media-library --no-qt-privacy-ask --no-interact --no-qt-name-in-title --ignore-config --loop -f \"bv*[vcodec=h264][height<=720]+ba\"";

            btnPlay.Enabled = false;
            btnStop.Enabled = true;
            lblStatus.Text = "Playing video...";

            // UseShellExecute = true is more correct for launching external apps that may show UI,
            // but keep UseShellExecute=false if you need stream redirection in the future.
            Process.Start(new ProcessStartInfo
            {
                FileName = vlc,
                Arguments = args,
                UseShellExecute = false
            });
        }

        void StopVlc()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/IM vlc.exe /F",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch
            {
                // best-effort stop; ignore failures
            }

            lblStatus.Text = "Stopping...";
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
        }
        private void SearchChose(object sender, EventArgs e)
        {
            if (!txtUrl.Text.StartsWith("https://"))
            {
                string id = txtUrl.Text;
                string imgurl = $"https://i.ytimg.com/vi/{id}/maxresdefault.jpg";
                pictureBox1.LoadAsync(imgurl);
                txtUrl.Text = "https://www.youtube.com/watch?v=" + id;
            }
        }
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
}
