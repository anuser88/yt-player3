using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace youtube
{
    public partial class Search : Form
    {
        HttpClient http = new HttpClient();

        public Search()
        {
            InitializeComponent();

            Text = "YouTube Search";
            Width = 800;
            Height = 600;
            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            TextBox txtSearch = new TextBox { Dock = DockStyle.Top };
            Button btnSearch = new Button { Text = "Search", Dock = DockStyle.Top, Height = 30 };
            ListView lv = new ListView { Dock = DockStyle.Fill };

            lv.View = View.Details;
            lv.FullRowSelect = true;
            lv.Columns.Add("Title", 450);
            lv.Columns.Add("ID (double-click to choose)", 300);

            Controls.Add(lv);
            Controls.Add(btnSearch);
            Controls.Add(txtSearch);

            http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            btnSearch.Click += async (_, __) =>
            {
                btnSearch.Enabled = false;
                lv.Items.Clear();
                await SearchYouTube(txtSearch.Text, lv);

                btnSearch.Enabled = true;
            };

            lv.DoubleClick += (_, __) =>
            {
                if (lv.SelectedItems.Count == 0) return;
                string id = lv.SelectedItems[0].SubItems[1].Text;
                Program.MainForm!.TxtUrlText = id;
                Program.MainForm!.VideoTitle = lv.SelectedItems[0].SubItems[0].Text;
            };
            this.FormClosed += (_, __) =>
            {
                Program.MainForm!.SearchOpened = false;
            };
        }

        async Task SearchYouTube(string query, ListView lv)
        {
            string q = Uri.EscapeDataString(query);
            string html = await http.GetStringAsync($"https://www.youtube.com/results?search_query={q}");

            string? json = ExtractInitialData(html);
            if (json == null) return;

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            JsonElement sections =
                root.GetProperty("contents")
                    .GetProperty("twoColumnSearchResultsRenderer")
                    .GetProperty("primaryContents")
                    .GetProperty("sectionListRenderer")
                    .GetProperty("contents");

            foreach (JsonElement section in sections.EnumerateArray())
            {
                if (!section.TryGetProperty("itemSectionRenderer", out JsonElement itemSection))
                    continue;

                foreach (JsonElement item in itemSection.GetProperty("contents").EnumerateArray())
                {
                    if (!item.TryGetProperty("videoRenderer", out JsonElement video))
                        continue;

                    string? id = video.GetProperty("videoId").GetString();
                    if (string.IsNullOrEmpty(id))
                        continue;
                    string title =
                        video.GetProperty("title")
                             .GetProperty("runs")[0]
                             .GetProperty("text")
                             .GetString() ?? String.Empty;
                    ListViewItem lvi = new ListViewItem(title);
                    lvi.SubItems.Add(id);
                    lv.Items.Add(lvi);
                }
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Search
            // 
            ClientSize = new Size(363, 281);
            Name = "Search";
            ResumeLayout(false);

        }

        static string? ExtractInitialData(string html)
        {
            const string key = "var ytInitialData = ";
            int start = html.IndexOf(key);
            if (start == -1) return null;

            start += key.Length;
            int end = html.IndexOf(";</script>", start);
            return end == -1 ? null : html.Substring(start, end - start);
        }
    }
}
