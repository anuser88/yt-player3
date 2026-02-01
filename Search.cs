using System.Text.Json;

namespace youtube
{
    public partial class Search : Form
    {
        readonly HttpClient http = new();

        public Search()
        {
            InitializeComponent();

            Text = "YouTube Search";
            Width = 800;
            Height = 600;
            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            TextBox txtSearch = new(){ Dock = DockStyle.Top };
            Button btnSearch = new() { Text = "Search", Dock = DockStyle.Top, Height = 30 };
            ListView lv = new()
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            lv.Columns.Add("#", 20);
            lv.Columns.Add("Name", 450);
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
                ListViewItem sel = lv.SelectedItems[0];
                string id = sel.SubItems[2].Text;
                Program.MainForm!.TxtUrlText = id;
                Program.MainForm!.VideoTitle = sel.SubItems[1].Text;
                JsonElement videoInfo = Items[int.Parse(sel.SubItems[0].Text) - 1];
                Program.MainForm!.AvatarUrl =
                    videoInfo.GetProperty("avatar")
                         .GetProperty("decoratedAvatarViewModel")
                         .GetProperty("avatar")
                         .GetProperty("avatarViewModel")
                         .GetProperty("image")
                         .GetProperty("sources")[0]
                         .GetProperty("url")
                         .GetString() ?? string.Empty;
                Program.MainForm!.ViewsText = 
                    videoInfo.GetProperty("shortViewCountText")
                         .GetProperty("simpleText")
                         .GetString() ?? string.Empty;
            };
            FormClosed += (_, __) =>
            {
                Program.MainForm!.SearchOpened = false;
            };
        }

        async Task SearchYouTube(string query, ListView lv)
        {
            Items = [];
            int idx = 1;
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
                    System.Diagnostics.Debug.WriteLine(video.ToString());
                    Items.Add(video.Clone());
                    string? id = video.GetProperty("videoId").GetString();
                    if (string.IsNullOrEmpty(id))
                        continue;
                    string title =
                        video.GetProperty("title")
                             .GetProperty("runs")[0]
                             .GetProperty("text")
                             .GetString() ?? string.Empty;
                    ListViewItem lvi = new(idx.ToString());
                    lvi.SubItems.Add(title);
                    lvi.SubItems.Add(id);
                    lv.Items.Add(lvi);
                    idx++;
                }
            }
        }
        public List<JsonElement> Items = [];
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
            return end == -1 ? null : html.AsSpan(start, end - start).ToString();
        }
    }
}
