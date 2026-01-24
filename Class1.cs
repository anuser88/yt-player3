using System.Diagnostics;

namespace youtube
{
    public partial class Setup
    {
        [STAThread]
        public static void Man()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private static readonly HttpClient s_httpClient = new HttpClient();

        private readonly Button btnSetup = new Button();
        private readonly Button btnCancel = new Button();
        private readonly ProgressBar progress = new ProgressBar();
        private readonly Label lbl = new Label();

        private CancellationTokenSource? _cts;

        public MainForm()
        {
            Text = "Setup";
            Width = 400;
            Height = 180;

            btnSetup.Text = "Check and Install Dependencies";
            btnSetup.Dock = DockStyle.Top;
            btnSetup.Height = 40;

            btnCancel.Text = "Cancel";
            btnCancel.Dock = DockStyle.Top;
            btnCancel.Height = 30;
            btnCancel.Enabled = false;

            progress.Dock = DockStyle.Top;
            progress.Height = 25;
            progress.Minimum = 0;
            progress.Maximum = 100;
            progress.Value = 0;
            progress.Style = ProgressBarStyle.Blocks;

            lbl.Text = "Idle";
            lbl.Dock = DockStyle.Top;
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            ControlBox = false;

            btnSetup.Click += async (_, __) =>
            {
                btnSetup.Enabled = false;
                btnCancel.Enabled = true;
                _cts = new CancellationTokenSource();

                try
                {
                    await EnsureDependencies(_cts.Token);
                    lbl.Text = "Close this window";
                    btnSetup.Text = "Done 👍";
                    ControlBox = true;
                }
                catch (OperationCanceledException)
                {
                    lbl.Text = "Cancelled";
                    btnSetup.Text = "Cancelled";
                }
                catch (Exception ex)
                {
                    lbl.Text = "Error: " + ex.Message;
                    btnSetup.Text = "Failed";
                }
                finally
                {
                    btnCancel.Enabled = false;
                    btnSetup.Enabled = true;
                    _cts.Dispose();
                    _cts = null;
                }
            };

            btnCancel.Click += (_, __) =>
            {
                btnCancel.Enabled = false;
                _cts?.Cancel();
            };

            Controls.Add(lbl);
            Controls.Add(progress);
            Controls.Add(btnCancel);
            Controls.Add(btnSetup);
        }

        // ================= CORE =================

        private async Task EnsureDependencies(CancellationToken ct)
        {
            // install yt-dlp (fast IO-bound download)
            await InstallYtDlp(ct);

            // install VLC if not present; installer run is CPU/IO and blocking, so run on background thread
            if (!HasVlc())
            {
                string installer = await DownloadVlcInstaller(ct);
                lbl.Text = "Installing VLC...";
                progress.Style = ProgressBarStyle.Marquee;
                // installer execution isn't cancellable here, but we avoid blocking UI by running on background thread
                await Task.Run(() => InstallVlc(installer), ct).ConfigureAwait(false);
                progress.Style = ProgressBarStyle.Blocks;
                progress.Value = 100;
            }
        }

        // ================= yt-dlp =================

        private async Task InstallYtDlp(CancellationToken ct)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            if (File.Exists(path)) return;

            lbl.Text = "Downloading yt-dlp...";
            progress.Value = 0;
            await DownloadFile(
                "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe",
                path,
                ct
            );
        }

        // ================= VLC =================

        private static bool HasVlc()
        {
            return File.Exists(@"C:\Program Files\VideoLAN\VLC\vlc.exe") ||
                   File.Exists(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe");
        }

        private async Task<string> DownloadVlcInstaller(CancellationToken ct)
        {
            string installer = Path.Combine(Path.GetTempPath(), "vlcsetup.exe");

            if (!File.Exists(installer))
            {
                lbl.Text = "Downloading VLC...";
                progress.Value = 0;
                await DownloadFile(
                    "https://get.videolan.org/vlc/3.0.23/win64/vlc-3.0.23-win64.exe",
                    installer,
                    ct
                );
            }

            return installer;
        }

        private void InstallVlc(string installerPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = "/S",
                UseShellExecute = true,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            p?.WaitForExit();
        }

        // ================= Utils =================

        private async Task DownloadFile(string url, string output, CancellationToken ct)
        {
            // Ensure target directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? AppDomain.CurrentDomain.BaseDirectory);

            progress.Value = 0;

            try
            {
                using var response = await s_httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes > 0;

                using var contentStream = await response.Content.ReadAsStreamAsync(ct);
                using var fileStream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                var buffer = new byte[81920];
                long totalRead = 0;
                int read;
                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read, ct);
                    totalRead += read;
                    if (canReportProgress)
                    {
                        int percent = (int)((totalRead * 100) / totalBytes);
                        // Update UI on UI thread (we're already on UI context because ConfigureAwait(true) used)
                        progress.Value = Math.Min(percent, 100);
                        lbl.Text = $"Downloading... {percent}%";
                    }
                }

                // final progress update
                progress.Value = 100;
                lbl.Text = "Download complete";
            }
            catch (OperationCanceledException)
            {
                // cleanup partial file on cancel
                try { if (File.Exists(output)) File.Delete(output); } catch { }
                progress.Value = 0;
                lbl.Text = "Download cancelled";
                throw;
            }
            catch (Exception ex)
            {
                progress.Value = 0;
                lbl.Text = "Download failed: " + ex.Message;
                throw;
            }
        }
    }
}
