using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;

namespace youtube
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        // cache VLC path lookup to avoid repeated IO checks
        private readonly Lazy<string?> _vlcPath = new(() =>
        {
            const string p1 = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            const string p2 = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";
            if (File.Exists(p1)) return p1;
            if (File.Exists(p2)) return p2;
            return null;
        });
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TxtUrlText
        {
            get => txtUrl.Text;
            set => txtUrl.Text = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string VideoTitle
        {
            get => VidTitle.Text;
            set => VidTitle.Text = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SearchOpened
        {
            get;
            set => openSearch.Enabled = !value;
        } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AvatarUrl
        {
            get => avatar.ImageLocation ?? string.Empty;
            set => avatar.LoadAsync(value);
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string OwnerNameText
        {
            get => OwnerName.Text;
            set => OwnerName.Text = value;
        }
        private string _channelUrl = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ChannelUrl
        {
            get => _channelUrl;
            set => _channelUrl = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ViewsText
        {
            get => Views.Text;
            set => Views.Text = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PubTimeText
        {
            get => PubTime.Text;
            set => PubTime.Text = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string VideoLenText
        {
            get => LengthText.Text;
            set => LenTextSet(value);
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color VideoLenColor
        {
            get => LengthText.BackColor;
            set => LengthText.BackColor = value;
        }
        private void LenTextSet(string t)
        {
            LengthText.Text = t;
            LengthText.Left = 480 - LengthText.Width;
        }
        // color animation - keeps UI responsive since it awaits Task.Delay
        private async void Form1_Load(object sender, EventArgs e)
        {
            Round_avatar(sender, e);
            Search search = new();
            search.Show();
            while (!IsDisposed)
            {
                for (int i = 0; i < 180 && !IsDisposed; i++)
                {
                    title.ForeColor = HsvToArgb(i * 2, 1, 1);
                    await Task.Delay(30).ConfigureAwait(true);
                }
            }
        }

        // async, non-blocking, and can be cancelled via token
        private static async Task<(string video, string audio)> GetStreamUrlsAsync(string url)
        {
            ProcessStartInfo psi = new()
            {
                FileName = "yt-dlp",
                Arguments = $"-f bv*+ba/b -g \"{url}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(psi);
            if (process == null)
                return (string.Empty, string.Empty);

            try
            {
                // Read output asynchronously and wait for exit. If cancelled, kill process.
                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task exitTask = process.WaitForExitAsync();

                // Wait for both tasks; if token is cancelled WaitForExitAsync will throw OperationCanceledException
                await Task.WhenAll(outputTask, exitTask).ConfigureAwait(false);

                string output = outputTask.Result ?? string.Empty;
                string[] lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

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

        private async void BtnPlay_Click(object sender, EventArgs e)
        {
            string url = TxtUrlText.Trim();
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

                (string video, string audio) = stream;

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

        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopVlc();
        }

        void StartVlc(string videoUrl, string audioUrl)
        {
            string? vlc = _vlcPath.Value;
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
            if (!TxtUrlText.StartsWith("https://"))
            {
                string id = TxtUrlText;
                string imgurl = $"https://i.ytimg.com/vi/{id}/maxresdefault.jpg";
                previewImg.LoadAsync(imgurl);
                TxtUrlText = "https://www.youtube.com/watch?v=" + id;
            }
        }
        public static Color HsvToArgb(float h, float s, float v, int a = 255)
        {
            h %= 360;
            if (h < 0) h += 360;

            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
            float m = v - c;

            float r, g, b;

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

        private void OpenSearch_Click(object sender, EventArgs e)
        {
            Search search = new();
            search.Show();
            SearchOpened = true;
        }
        private void Round_avatar(object sender, EventArgs e)
        {
            GraphicsPath gp = new();
            gp.AddEllipse(0, 0, avatar.Width, avatar.Height);
            avatar.Region = new Region(gp);
        }
        private void GotoChannel(object sender, EventArgs e)
        {
            if (_channelUrl == string.Empty)
                return;
            string channelUrl = $"https://www.youtube.com{_channelUrl}";
            Process.Start(new ProcessStartInfo
            {
                FileName = channelUrl,
                UseShellExecute = true
            });
        }
        private static string GetSaveDirectory()
        {
            SaveFileDialog sfd = new()
            {
                FileName = "this name doesnt work lol",
                Filter = "WEBM Files |*.webm",
                OverwritePrompt = false
            };


            if (sfd.ShowDialog() == DialogResult.OK)
            {
                return Path.GetDirectoryName(sfd.FileName)!;
            }
            return string.Empty;
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            if (TxtUrlText == string.Empty)
                return;
            string SaveDir = GetSaveDirectory();
            Process proc = new() { StartInfo = new ProcessStartInfo()
            {
                FileName = "yt-dlp",
                Arguments = $"-f \"bv*+ba/b\" -N 8 -o \"{SaveDir}\\%(title)s.%(ext)s\" \"{TxtUrlText}\"",
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = false
            }};
            proc.Start();
            DownloadButton.Enabled = false;
            lblStatus.Text = "Downloading...";
            proc.WaitForExit();
            DownloadButton.Enabled = true;
            lblStatus.Text = "Download complete!";
        }
    }
}
