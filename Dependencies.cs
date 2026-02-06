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
        private static readonly HttpClient s_httpClient = new();

        private readonly Button btnSetup = new();
        private readonly Button btnCancel = new();
        private readonly ProgressBar progress = new();
        private readonly Label lbl = new();

        private CancellationTokenSource? _cts;

        public MainForm()
        {
            Text = "Setup";
            Width = 400;
            Height = 180;

            btnSetup.Text = "";
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

            lbl.Text = "Check and Install Dependencies";
            lbl.Dock = DockStyle.Top;
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            //ControlBox = false;

            Load += async (_, __) =>
            {
                btnSetup.Enabled = false;
                btnCancel.Enabled = true;
                _cts = new CancellationTokenSource();

                try
                {
                    lbl.Text = "Do not close this window";
                    await EnsureDependencies(_cts.Token);
                    Close();
                }
                catch (OperationCanceledException)
                {
                    lbl.Text = "Cancelled";
                    btnSetup.Text = "Retry";
                }
                catch (Exception ex)
                {
                    lbl.Text = "Error: " + ex.Message;
                    btnSetup.Text = "Retry";
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
            if (File.Exists(path))
            {
                ProcessStartInfo psi = new()
                {
                    FileName = path,
                    Arguments = "-U",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };Process.Start(psi)?.WaitForExit();
                return;
            }

            lbl.Text = "Downloading yt-dlp...";
            progress.Value = 0;
            await DownloadFile(
                "https://github.com/anuser88/yt-dlp-fork/releases/latest/download/yt-dlp.exe",
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

        private static void InstallVlc(string installerPath)
        {
            ProcessStartInfo psi = new()
            {
                FileName = installerPath,
                Arguments = "/S",
                UseShellExecute = true,
                CreateNoWindow = true
            };

            using Process? p = Process.Start(psi);
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
                using HttpResponseMessage response = await s_httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                long totalBytes = response.Content.Headers.ContentLength ?? -1L;
                bool canReportProgress = totalBytes > 0;

                using Stream contentStream = await response.Content.ReadAsStreamAsync(ct);
                using FileStream fileStream = new(output, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                byte[] buffer = new byte[81920];
                long totalRead = 0;
                int read;
                while ((read = await contentStream.ReadAsync(buffer.AsMemory(), ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read), ct);
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
