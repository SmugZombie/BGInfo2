using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace BgInfoClone
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        // Core application fields
        private string selectedWallpaperPath = string.Empty;
        private List<ApiConnection> apiConnections = new();
        private Point dragOffset;
        private Font selectedFont = new Font("Consolas", 20);
        private Brush selectedBrush = Brushes.LightGreen;
        private ColorDialog colorDialog = new ColorDialog();
        private FontDialog fontDialog = new FontDialog();
        private System.Windows.Forms.Timer updateTimer;
        private const string ConfigPath = "bginfo_config.json";
        private int previewWidth;
        private int previewHeight;
        private bool startHidden = false;
        private int refreshIntervalMinutes = 30; // Default 30 minutes

        // System tray fields
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;

        public MainForm()
        {
            // Load API connections before UI initialization
            LoadApiConnections();
            
            // Calculate preview dimensions
            float scale = 0.5f;
            previewWidth = (int)(screenWidth * scale);
            previewHeight = (int)(screenHeight * scale);
            
            // Initialize UI components (from designer)
            InitializeComponent();
            
            // Initialize timer after UI is ready
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 30 * 60 * 1000; // 30 minutes
            updateTimer.Tick += (s, e) => GenerateWallpaper();
            updateTimer.Start();
            
            // Load configuration after UI is initialized
            LoadConfig();
            
            // Initialize tray icon
            InitializeTrayIcon();
            
            // Handle form events for tray behavior
            this.FormClosing += MainForm_FormClosing;
            this.Load += MainForm_Load;
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
                {
                    // Create a new bitmap with screen resolution for consistent scaling
                    using (var screenSizedBitmap = new Bitmap(screenWidth, screenHeight))
                    using (var graphics = Graphics.FromImage(screenSizedBitmap))
                    {
                        // Draw the original wallpaper stretched to screen size
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(original, 0, 0, screenWidth, screenHeight);
                        
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        string info = GetSystemInfo();
                        
                        // Calculate scaling ratios from preview to screen coordinates
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

                        screenSizedBitmap.Save(newImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }

                // Update preview with the generated wallpaper
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
            string result;

            int timeoutSeconds = 5; // default timeout

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

                string url = conn.Url;
                var contentType = conn.ContentType;
                var jsonKey = conn.JsonKey;
                var regexPattern = conn.RegexPattern;
                var method = conn.Method.ToUpper();
                var authType = conn.AuthType;
                var username = conn.Username;
                var passwordOrToken = conn.PasswordOrToken;

                try
                {
                    if (System.IO.File.Exists(url))
                {
                    result = System.IO.File.ReadAllText(url);
                }
                else if (url.StartsWith("cmd://", StringComparison.OrdinalIgnoreCase))
                {
                    string command = url.Substring(6);
                    var processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + command)
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var process = System.Diagnostics.Process.Start(processInfo);
                    result = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }
                else
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                    if (authType == "Basic")
                    {
                        var byteArray = Encoding.ASCII.GetBytes($"{username}:{passwordOrToken}");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }
                    else if (authType == "Bearer")
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", passwordOrToken);
                    }

                    HttpResponseMessage response = method == "POST"
                        ? client.PostAsync(url, null).Result
                        : client.GetAsync(url).Result;

                    result = response.Content.ReadAsStringAsync().Result;
                }

                string output = result;

                if (contentType == "json" && !string.IsNullOrWhiteSpace(jsonKey))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(result);
                        if (TryExtractJsonValue(doc.RootElement, jsonKey.Split('.'), out var extracted))
                        {
                            output = extracted;
                        }
                        else
                        {
                            output = "(no match)";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("JSON path error: " + ex.Message);
                        output = "(json path error)";
                    }
                }
                else if (contentType == "text" && !string.IsNullOrWhiteSpace(regexPattern))
                {
                    try
                    {
                        var regex = new Regex(regexPattern, RegexOptions.Multiline);
                        var match = regex.Match(result);
                        if (match.Success)
                        {
                            // Use first capturing group if exists, else whole match
                            output = match.Groups.Count > 1 && !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Value;
                        }
                        else
                        {
                            output = "(no match)";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Regex error: " + ex.Message);
                        output = "(regex error)";
                    }
                }
                else if (contentType == "text" && string.IsNullOrWhiteSpace(regexPattern))
                {
                    // If no regex pattern is provided, just return the raw text
                    output = result;
                }

                string value = output.Trim();
                info = info.Replace(tag, value);
                    /*using var client = new HttpClient();

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

                    // Debug logging
                    System.Diagnostics.Debug.WriteLine($"API '{conn.Name}' response: {result}");

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
                        try
                        {
                            if(conn.RegexPattern == null || conn.RegexPattern.Trim() == "")
                            {
                                value = result; // No regex, use raw text
                            }else{
                                var match = Regex.Match(result, conn.RegexPattern, RegexOptions.Multiline);
                                if (match.Success)
                                {
                                    value = match.Groups.Count > 1 && !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Value;
                                }
                                else
                                {
                                    value = "(no match)";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Regex error for API '{conn.Name}': {ex.Message}");
                            value = "(regex error)";
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"API '{conn.Name}' extracted value: {value}");

                    info = info.Replace(tag, value);
                    */
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing API '{conn.Name}': {ex.Message}");
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
            try
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
                    customFormatText = formatBox.Text,
                    startHidden,
                    refreshIntervalMinutes
                };

                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save configuration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return;
            try
            {
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

                // Load startHidden setting
                if (config.TryGetProperty("startHidden", out var sh))
                {
                    startHidden = sh.GetBoolean();
                }

                UpdateStatusBar(); 
                UpdatePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load configuration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        #region System Tray Functionality

        private void InitializeTrayIcon()
        {
            try
            {
                trayMenu = new ContextMenuStrip();

                // Create tray menu items
                // Refresh Wallpaper
                var refreshItem = new ToolStripMenuItem("Refresh Wallpaper");
                refreshItem.Click += (s, e) => {
                    try { GenerateWallpaper(true); }
                    catch (Exception ex) { MessageBox.Show("Error refreshing wallpaper: " + ex.Message); }
                };

                // Show/Hide GUI
                var toggleGuiItem = new ToolStripMenuItem("Show/Hide GUI");
                toggleGuiItem.Click += (s, e) => {
                    if (this.Visible)
                    {
                        this.Hide();
                        this.ShowInTaskbar = false;
                    }
                    else
                    {
                        this.Show();
                        this.WindowState = FormWindowState.Normal;
                        this.ShowInTaskbar = true;
                        this.BringToFront();
                    }
                };

                // Run on Startup (toggle)
                var runStartupItem = new ToolStripMenuItem("Run on Startup")
                {
                    CheckOnClick = true
                };
                // Set initial state from registry
                runStartupItem.Checked = IsStartupEnabled();
                runStartupItem.Click += (s, e) =>
                {
                    bool enable = runStartupItem.Checked;
                    try
                    {
                        UpdateStartupRegistry(enable);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating startup setting: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                // Quit
                var quitItem = new ToolStripMenuItem("Quit");
                quitItem.Click += (s, e) => {
                    trayIcon.Visible = false;
                    Application.Exit();
                };

                // Add items to the tray menu
                trayMenu.Items.AddRange(new ToolStripItem[] { refreshItem, toggleGuiItem, runStartupItem, quitItem });

                // Initialize NotifyIcon
                trayIcon = new NotifyIcon
                {
                    Text = "BGInfoClone",
                    Icon = SystemIcons.Application,
                    ContextMenuStrip = trayMenu,
                    Visible = true
                };

                // Double-click toggles window visibility
                trayIcon.DoubleClick += (s, e) => {
                    if (this.Visible)
                    {
                        this.Hide();
                        this.ShowInTaskbar = false;
                    }
                    else
                    {
                        this.Show();
                        this.WindowState = FormWindowState.Normal;
                        this.ShowInTaskbar = true;
                        this.BringToFront();
                    }
                };
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error initializing tray icon: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (startHidden)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Hide();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Hide to tray instead of closing
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }

        private bool IsStartupEnabled()
        {
            try 
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    if (key == null) return false;
                    string value = key.GetValue("BGInfoClone") as string;
                    return !string.IsNullOrEmpty(value);
                }
            }
            catch { return false; }
        }

        private void UpdateStartupRegistry(bool enable)
        {
            try 
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null) throw new Exception("Unable to open registry key.");
                    if (enable)
                    {
                        // Use the path to the executable; ensure it is properly quoted if it contains spaces.
                        string exePath = "\"" + Application.ExecutablePath + "\"";
                        key.SetValue("BGInfoClone", exePath);
                    }
                    else
                    {
                        key.DeleteValue("BGInfoClone", false);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Failed to update startup registry: " + ex.Message);
            }
        }

        #endregion
    }

    static class Program
    {
        private static System.Threading.Mutex? mutex;

        [STAThread]
        static void Main()
        {
            bool createdNew;
            mutex = new System.Threading.Mutex(true, "BgInfoCloneSingletonMutex", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Another instance of BGInfoClone is already running.", "Instance Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            mutex.ReleaseMutex();
        }
    }
}
