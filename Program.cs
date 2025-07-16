using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;

namespace BgInfoClone
{
    public class MainForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        private string selectedWallpaperPath = string.Empty;
        private List<ApiConnection> apiConnections = new();
        private PictureBox previewBox;
        private Panel dragPanel;
        private Point dragOffset;
        private TextBox formatBox;
        private Button fontButton, colorButton;
        private Font selectedFont = new Font("Consolas", 20);
        private Brush selectedBrush = Brushes.LightGreen;
        private ColorDialog colorDialog = new ColorDialog();
        private FontDialog fontDialog = new FontDialog();
        private System.Windows.Forms.Timer updateTimer;
        private const string ConfigPath = "bginfo_config.json";
        private CheckBox chkHostname;
        private CheckBox chkUser;
        private CheckBox chkIP;
        private CheckBox chkOS;
        private CheckBox chkCores;
        private int previewWidth;
        private int previewHeight;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel previewPosLabel;
        private ToolStripStatusLabel realPosLabel;


        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;

        public MainForm()
        {
            LoadApiConnections(); // ✅ Load from file before it's used

                    
            float scale = 0.5f;
            previewWidth = (int)(screenWidth * scale);
            previewHeight = (int)(screenHeight * scale);
            
            this.Text = "BGInfo 2";
            this.Size = new Size((int)previewWidth + 45, (int)previewHeight + 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;
            this.AutoScroll = true;

            var layout = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };

            chkHostname = new CheckBox { Text = "Hostname", Checked = true };
            chkUser = new CheckBox { Text = "User", Checked = true };
            chkIP = new CheckBox { Text = "IP", Checked = true };
            chkOS = new CheckBox { Text = "OS", Checked = true };
            chkCores = new CheckBox { Text = "Cores", Checked = true };

            fontButton = new Button { Text = "Choose Font" };
            fontButton.Click += (s, e) => {
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFont = fontDialog.Font;
                }
            };

            colorButton = new Button { Text = "Choose Color" };
            colorButton.Click += (s, e) => {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedBrush = new SolidBrush(colorDialog.Color);
                }
            };

            var manageApisButton = new Button { Text = "Manage APIs" };
            manageApisButton.Click += (s, e) =>
            {
                var form = new ApiManagerForm(apiConnections);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    apiConnections = form.UpdatedConnections;
                    SaveApiConnections();
                }
            };
            layout.Controls.Add(manageApisButton);

            Button selectButton = new Button() { Text = "Select Wallpaper", Width = 100 };
            selectButton.Click += SelectButton_Click;
            layout.Controls.Add(selectButton);

            Button generateButton = new Button() { Text = "Update Wallpaper", Width = 100 };
            generateButton.Click += GenerateButton_Click;
            layout.Controls.Add(generateButton);

            Button saveButton = new Button() { Text = "Save Config", Width = 100 };
            saveButton.Click += (s, e) => SaveConfig();
            layout.Controls.Add(saveButton);

            Button loadButton = new Button() { Text = "Load Config", Width = 100 };
            loadButton.Click += (s, e) => LoadConfig();
            layout.Controls.Add(loadButton);

            layout.Controls.Add(new Label { Text = "Template Format:" });
            formatBox = new TextBox { Width = 600, Multiline = true, Height = 60 };
            formatBox.Text = @"Hostname: {hostname}
User: {user}
IP: {ip}
OS: {os}
Cores: {cores}";
            layout.Controls.Add(formatBox);

            Controls.Add(layout);

            dragPanel = new Panel
            {
                BackColor = Color.Transparent,
                Location = new Point(0, 0), // Relative to previewBox now
                Size = new Size(500, 100),
                Parent = previewBox // 👈 This is the key change
            };
            dragPanel.BringToFront();
            dragPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var pen = new Pen(Color.Red, 2);
                g.DrawRectangle(pen, 0, 0, dragPanel.Width - 1, dragPanel.Height - 1);

                // Draw the preview text
                string previewText = GetSystemInfo();
                using var brush = new SolidBrush(Color.Red);
                Color background = Color.FromArgb(100, 255, 255, 255); // translucent white
                e.Graphics.FillRectangle(new SolidBrush(background), 0, 0, dragPanel.Width, dragPanel.Height);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                //g.DrawString(previewText, selectedFont, brush, new PointF(5, 5));
                g.DrawString(previewText, selectedFont, brush, new PointF(0, 0));

            };

            dragPanel.MouseDown += (s, e) => dragOffset = e.Location;
            dragPanel.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    var newLeft = dragPanel.Left + e.X - dragOffset.X;
                    var newTop = dragPanel.Top + e.Y - dragOffset.Y;

                    // Clamp inside previewBox
                    newLeft = Math.Max(0, Math.Min(newLeft, previewBox.Width - dragPanel.Width));
                    newTop = Math.Max(0, Math.Min(newTop, previewBox.Height - dragPanel.Height));

                    dragPanel.Left = newLeft;
                    dragPanel.Top = newTop;

                    dragPanel.Invalidate();
                    UpdateStatusBar(); // Update position in status bar
                }
            };

            Controls.Add(dragPanel);

            int previewScalePercent = 50; // you can tweak this later
            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            int previewW = (screenW * previewScalePercent) / 100;
            int previewH = (screenH * previewScalePercent) / 100;

            previewBox = new PictureBox
            {
                Width = previewW,
                Height = previewH,
                SizeMode = PictureBoxSizeMode.Normal,
                Location = new Point(10, layout.Bottom + 20)
            };


            previewBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var pen = new Pen(Color.Red, 2);
                g.DrawRectangle(pen, 0, 0, previewBox.Width - 1, previewBox.Height - 1);
            };
            Controls.Add(previewBox);

            statusStrip = new StatusStrip();
            previewPosLabel = new ToolStripStatusLabel();
            realPosLabel = new ToolStripStatusLabel();

            statusStrip.Items.Add(previewPosLabel);
            statusStrip.Items.Add(realPosLabel);

            Controls.Add(statusStrip);

            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 30 * 60 * 1000; // 30 minutes
            updateTimer.Tick += (s, e) => GenerateWallpaper();
            updateTimer.Start();

            
            LoadConfig();
        }

        private void SelectButton_Click(object? sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Wallpaper Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedWallpaperPath = openFileDialog.FileName;
                UpdatePreview();
            }
        }

        private void GenerateButton_Click(object? sender, EventArgs e)
        {
            GenerateWallpaper(true);
        }

        private void GenerateWallpaper(bool showMessage = false)
        {
            try
            {
                string wallpaperPath = string.IsNullOrEmpty(selectedWallpaperPath) ? GetCurrentWallpaper() : selectedWallpaperPath;
                if (string.IsNullOrEmpty(wallpaperPath) || !File.Exists(wallpaperPath))
                {
                    if (showMessage)
                        MessageBox.Show($"Could not locate wallpaper file.\nPath: {wallpaperPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string newImagePath = Path.Combine(Path.GetTempPath(), "bginfo_clone_wallpaper.jpg");

                using (var original = new Bitmap(wallpaperPath))
                using (var graphics = Graphics.FromImage(original))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    string info = GetSystemInfo();
                    
                    float xRatio = (float)screenWidth / previewBox.Width;
                    float yRatio = (float)screenHeight / previewBox.Height;

                    Point scaledLocation = new Point(
                        (int)(dragPanel.Left * xRatio),
                        (int)(dragPanel.Top * yRatio)
                    );


                    graphics.DrawString(info, selectedFont, selectedBrush, scaledLocation);

                    if (File.Exists(newImagePath))
                    {
                        try { File.Delete(newImagePath); } catch { }
                    }

                    original.Save(newImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    // Ensure we dispose previous image to unlock the file
                    if (previewBox.Image != null)
                    {
                        previewBox.Image.Dispose();
                        previewBox.Image = null;
                    }

                    using (var fs = new FileStream(newImagePath, FileMode.Open, FileAccess.Read))
                    using (var originalBitmap = new Bitmap(fs))
                    {
                        Bitmap scaled = new Bitmap(previewBox.Width, previewBox.Height);
                        using (Graphics g = Graphics.FromImage(scaled))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(originalBitmap, 0, 0, scaled.Width, scaled.Height);
                        }

                        previewBox.Image = scaled;
                    }


                }

                SetWallpaper(newImagePath);
                /*if (showMessage)
                    MessageBox.Show($"Wallpaper updated successfully.\nSaved to: {newImagePath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); */
            }
            catch (Exception ex)
            {
                if (showMessage)
                    MessageBox.Show($"Error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SetWallpaperStyleStretch()
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (key != null)
                {
                    key.SetValue("WallpaperStyle", "2");  // 2 = Stretch
                    key.SetValue("TileWallpaper", "0");   // 0 = No tile
                }
            }
        }


        private void UpdatePreview()
        {
            if (string.IsNullOrEmpty(selectedWallpaperPath) || !File.Exists(selectedWallpaperPath)) return;

            try
            {
                string tempPreviewPath = Path.Combine(Path.GetTempPath(), "bginfo_preview.jpg");

                float scale = 0.5f;
                int screenW = Screen.PrimaryScreen.Bounds.Width;
                int screenH = Screen.PrimaryScreen.Bounds.Height;

                int previewW = (int)(screenW * scale);
                int previewH = (int)(screenH * scale);

                previewBox.Size = new Size(previewW, previewH);

                // Load and draw onto a full-sized image first
                using (var original = new Bitmap(selectedWallpaperPath))
                using (var fullSize = new Bitmap(screenWidth, screenHeight))
                using (var g = Graphics.FromImage(fullSize))
                {
                    // Fill the screen with wallpaper (stretch if needed)
                    g.DrawImage(original, 0, 0, screenWidth, screenHeight);

                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                    string info = GetSystemInfo();

                    // Get dragPanel coordinates in full screen scale
                    float xRatio = (float)screenWidth / previewBox.Width;
                    float yRatio = (float)screenHeight / previewBox.Height;

                    Point scaledLocation = new Point(
                        (int)(dragPanel.Left * xRatio),
                        (int)(dragPanel.Top * yRatio)
                    );

                    //g.DrawString(info, selectedFont, selectedBrush, scaledLocation);

                    // Now scale that full-sized image to preview size
                    using (var preview = new Bitmap(previewWidth, previewHeight))
                    using (var pg = Graphics.FromImage(preview))
                    {
                        pg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        pg.DrawImage(fullSize, 0, 0, previewWidth, previewHeight);

                        // Save to temp file
                        if (File.Exists(tempPreviewPath)) File.Delete(tempPreviewPath);
                        preview.Save(tempPreviewPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }

                // Dispose old image if needed
                previewBox.Image?.Dispose();
                UpdateStatusBar(); 
                // Load the new scaled image
                using (var fs = new FileStream(tempPreviewPath, FileMode.Open, FileAccess.Read))
                {
                    previewBox.Image = new Bitmap(fs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Preview error: " + ex.Message);
            }
        }


        private readonly TextBox customTextBox = new TextBox { Width = 300 };
        private readonly List<(string url, string jsonKey)> apiCustomFields = new();

        private string GetSystemInfo()
        {
            string template = formatBox.Text;
            var hostname = Environment.MachineName;
            var user = Environment.UserName;
            var ip = GetLocalIPAddress();
            var os = Environment.OSVersion.ToString();
            var cores = Environment.ProcessorCount.ToString();

            string info = template
                .Replace("{hostname}", hostname)
                .Replace("{user}", user)
                .Replace("{ip}", ip)
                .Replace("{os}", os)
                .Replace("{cores}", cores);

            foreach (var conn in apiConnections)
            {
                string tag = $"{{API:{conn.Name}}}";

                if (!info.Contains(tag)) continue;

                try
                {
                    using var client = new HttpClient();

                    if (conn.AuthType == "Basic")
                    {
                        var byteArray = Encoding.ASCII.GetBytes($"{conn.Username}:{conn.PasswordOrToken}");
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }
                    else if (conn.AuthType == "Bearer")
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", conn.PasswordOrToken);
                    }

                    var requestTask = conn.Method == "POST"
                        ? client.PostAsync(conn.Url, null)
                        : client.GetAsync(conn.Url);

                    var response = requestTask.Result;
                    string result = response.Content.ReadAsStringAsync().Result;
                    string value = "(error)";

                    if (conn.ContentType == "json")
                    {
                        using var doc = JsonDocument.Parse(result);
                        if (TryExtractJsonValue(doc.RootElement, conn.JsonKey.Split('.'), out var extracted))
                        {
                            value = extracted;
                        }
                    }
                    else if (conn.ContentType == "text")
                    {
                        var match = Regex.Match(result, conn.RegexPattern);
                        if (match.Success)
                        {
                            value = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                        }
                        else
                        {
                            value = "(no match)";
                        }
                    }

                    info = info.Replace(tag, value);
                }
                catch
                {
                    info = info.Replace(tag, "(error)");
                }
            }

            if (!string.IsNullOrWhiteSpace(customTextBox.Text))
                info += $"\nNote: {customTextBox.Text}";

            return info.TrimEnd();
        }



        private static string GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "N/A";
        }

        private static void SetWallpaper(string imagePath)
        {
            SetWallpaperStyleStretch();

            bool result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            if (!result)
            {
                MessageBox.Show("Failed to set wallpaper via SystemParametersInfo", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string GetCurrentWallpaper()
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\\Desktop", false);
            return key?.GetValue("WallPaper")?.ToString() ?? "";
        }

        private static bool TryExtractJsonValue(JsonElement element, string[] path, out string result)
        {
            result = "";
            foreach (var key in path)
            {
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(key, out var sub))
                    element = sub;
                else
                    return false;
            }
            result = element.ToString();
            return true;
        }

        private void SaveConfig()
        {
            var config = new
            {
                selectedWallpaperPath,
                selectedFontName = selectedFont.Name,
                selectedFontSize = selectedFont.Size,
                selectedFontStyle = (int)selectedFont.Style,
                fontColor = ((SolidBrush)selectedBrush).Color.ToArgb(),
                dragLeft = dragPanel.Left,
                dragTop = dragPanel.Top,
                dragWidth = dragPanel.Width,
                dragHeight = dragPanel.Height,
                customFormatText = formatBox.Text
            };

            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config));
            //MessageBox.Show("Configuration saved.");
        }


        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return;
            var json = File.ReadAllText(ConfigPath);
            using var doc = JsonDocument.Parse(json);
            var config = doc.RootElement;

            selectedWallpaperPath = config.GetProperty("selectedWallpaperPath").GetString() ?? "";
            selectedFont = new Font(
                config.GetProperty("selectedFontName").GetString(),
                (float)config.GetProperty("selectedFontSize").GetDouble(),
                (FontStyle)config.GetProperty("selectedFontStyle").GetInt32()
            );
            selectedBrush = new SolidBrush(Color.FromArgb(config.GetProperty("fontColor").GetInt32()));

            if (config.TryGetProperty("dragLeft", out var l) &&
                config.TryGetProperty("dragTop", out var t) &&
                config.TryGetProperty("dragWidth", out var w) &&
                config.TryGetProperty("dragHeight", out var h))
            {
                dragPanel.Left = l.GetInt32();
                dragPanel.Top = t.GetInt32();
                dragPanel.Width = w.GetInt32();
                dragPanel.Height = h.GetInt32();
            }

            if (config.TryGetProperty("customFormatText", out var format))
            {
                formatBox.Text = format.GetString();
            }
            UpdateStatusBar(); 
            UpdatePreview();
        }


        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 400,
                    Height = 150,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };

                Label textLabel = new Label() { Left = 20, Top = 20, Text = text, Width = 340 };
                TextBox inputBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
                Button confirmation = new Button() { Text = "OK", Left = 280, Width = 80, Top = 80, DialogResult = DialogResult.OK };

                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text : "";
            }
        }

        private const string ApiConfigPath = "api_connections.json";

        private void LoadApiConnections()
        {
            if (File.Exists(ApiConfigPath))
            {
                var json = File.ReadAllText(ApiConfigPath);
                apiConnections = JsonSerializer.Deserialize<List<ApiConnection>>(json) ?? new List<ApiConnection>();
            }
        }

        private void SaveApiConnections()
        {
            var json = JsonSerializer.Serialize(apiConnections, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ApiConfigPath, json);
        }

        private void UpdateStatusBar()
        {
            // Drag position in previewBox coordinates
            int previewX = dragPanel.Left;
            int previewY = dragPanel.Top;

            // Scale to full screen coordinates
            float xRatio = (float)screenWidth / previewBox.Width;
            float yRatio = (float)screenHeight / previewBox.Height;

            int scaledX = (int)(previewX * xRatio);
            int scaledY = (int)(previewY * yRatio);

            // Update the status bar panels
            previewPosLabel.Text = $"Preview: {previewX}, {previewY}";
            realPosLabel.Text = $"Scaled: {scaledX}, {scaledY}";

        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
