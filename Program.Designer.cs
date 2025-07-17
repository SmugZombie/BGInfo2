using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace BgInfoClone
{
    public partial class MainForm
    {
        private MenuStrip menuStrip;
        private Panel topPanel;
        private Panel rightPanel;
        private ToolTip toolTip;
        
        // Control declarations
        private PictureBox previewBox;
        private Panel dragPanel;
        private TextBox formatBox;
        private ListBox availableVariablesListBox;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel previewPosLabel;
        private ToolStripStatusLabel realPosLabel;

        /// <summary>
        /// Method to initialize and arrange UI components.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Initialize tooltip
            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 100;
            
            // Initialize form properties
            this.Text = "BGInfo 2";
            this.Size = new Size(previewWidth + 350, previewHeight + 150); // Wider to accommodate right panel
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;
            this.AutoScroll = true;
            
            // Initialize components
            InitializeMenuAndTopPanel();
            InitializeRightPanel();
            InitializePreviewArea();
            InitializeStatusStrip();
            
            // Add controls to form in proper order
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(topPanel); // Contains both menu and action buttons
            this.Controls.Add(rightPanel);
            this.Controls.Add(previewBox);
            this.Controls.Add(statusStrip);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeMenuAndTopPanel()
        {
            // Create top panel that will contain both menu and action buttons
            topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 60;
            topPanel.BackColor = Color.FromArgb(240, 240, 240);
            
            // Create menu strip
            menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top;
            menuStrip.BackColor = Color.FromArgb(240, 240, 240);
            
            // File menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            
            ToolStripMenuItem selectWallpaperItem = new ToolStripMenuItem("🖼 Select Wallpaper");
            selectWallpaperItem.Click += SelectButton_Click;
            
            ToolStripMenuItem updateWallpaperItem = new ToolStripMenuItem("🔄 Update Wallpaper");
            updateWallpaperItem.Click += GenerateButton_Click;
            
            ToolStripMenuItem saveConfigItem = new ToolStripMenuItem("💾 Save Config");
            saveConfigItem.Click += (s, e) => SaveConfig();
            
            ToolStripMenuItem loadConfigItem = new ToolStripMenuItem("📂 Load Config");
            loadConfigItem.Click += (s, e) => LoadConfig();
            
            ToolStripMenuItem exitItem = new ToolStripMenuItem("❌ Exit");
            exitItem.Click += (s, e) => this.Close();
            
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                selectWallpaperItem, updateWallpaperItem, new ToolStripSeparator(),
                saveConfigItem, loadConfigItem, new ToolStripSeparator(), exitItem
            });
            
            // Edit menu
            ToolStripMenuItem editMenu = new ToolStripMenuItem("Edit");
            
            ToolStripMenuItem chooseFontItem = new ToolStripMenuItem("✏️ Choose Font");
            chooseFontItem.Click += (s, e) => {
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFont = fontDialog.Font;
                }
            };
            
            ToolStripMenuItem chooseColorItem = new ToolStripMenuItem("🎨 Choose Color");
            chooseColorItem.Click += (s, e) => {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedBrush = new SolidBrush(colorDialog.Color);
                }
            };
            
            editMenu.DropDownItems.AddRange(new ToolStripItem[] {
                chooseFontItem, chooseColorItem
            });
            
            // Tools menu
            ToolStripMenuItem toolsMenu = new ToolStripMenuItem("Tools");
            
            ToolStripMenuItem manageApisItem = new ToolStripMenuItem("⚙️ Manage APIs");
            manageApisItem.Click += (s, e) =>
            {
                var form = new ApiManagerForm(apiConnections);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    apiConnections = form.UpdatedConnections;
                    SaveApiConnections();
                    PopulateAvailableVariables(); // Refresh variables list
                }
            };

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("⚙️ Settings");
            settingsItem.Click += (s, e) =>
            {
                using var settingsForm = new SettingsForm(refreshIntervalMinutes);
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    refreshIntervalMinutes = settingsForm.RefreshIntervalMinutes;
                    updateTimer.Interval = refreshIntervalMinutes * 60 * 1000;
                    SaveConfig();
                }
            };
            
            toolsMenu.DropDownItems.Add(manageApisItem);
            toolsMenu.DropDownItems.Add(settingsItem);
            
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, toolsMenu });
            
            // Create action buttons panel within top panel
            FlowLayoutPanel actionButtonsPanel = new FlowLayoutPanel();
            actionButtonsPanel.Dock = DockStyle.Fill;
            actionButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
            actionButtonsPanel.Padding = new Padding(10, 5, 10, 5);
            actionButtonsPanel.BackColor = Color.FromArgb(240, 240, 240);
            
            // Create icon buttons
            Button btnSelect = CreateIconButton("🖼", "Select a wallpaper image file to use as background");
            btnSelect.Click += SelectButton_Click;
            
            Button btnGenerate = CreateIconButton("🔄", "Generate and apply wallpaper with system information overlay");
            btnGenerate.Click += GenerateButton_Click;
            
            Button btnSave = CreateIconButton("💾", "Save current configuration settings to file");
            btnSave.Click += (s, e) => SaveConfig();
            
            Button btnLoad = CreateIconButton("📂", "Load previously saved configuration settings");
            btnLoad.Click += (s, e) => LoadConfig();
            
            Button btnFont = CreateIconButton("✏️", "Choose font style and size for system information text");
            btnFont.Click += (s, e) => {
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFont = fontDialog.Font;
                }
            };
            
            Button btnColor = CreateIconButton("🎨", "Choose text color for system information display");
            btnColor.Click += (s, e) => {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedBrush = new SolidBrush(colorDialog.Color);
                }
            };
            
            Button btnManageApis = CreateIconButton("⚙️", "Manage API connections for external data sources");
            btnManageApis.Click += (s, e) =>
            {
                var form = new ApiManagerForm(apiConnections);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    apiConnections = form.UpdatedConnections;
                    SaveApiConnections();
                    PopulateAvailableVariables(); // Refresh variables list
                }
            };
            
            actionButtonsPanel.Controls.AddRange(new Control[] {
                btnSelect, btnGenerate, btnSave, btnLoad, btnFont, btnColor, btnManageApis
            });
            
            // Add menu and action buttons to top panel
            topPanel.Controls.Add(actionButtonsPanel);
            topPanel.Controls.Add(menuStrip);
        }

        private void InitializeRightPanel()
        {
            rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Right;
            rightPanel.Width = 320;
            rightPanel.BackColor = Color.FromArgb(250, 250, 250);
            rightPanel.Padding = new Padding(10);
            
            // Available Variables section
            Label variablesLabel = new Label();
            variablesLabel.Text = "Available Variables (Double-click to add):";
            variablesLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            variablesLabel.AutoSize = true;
            variablesLabel.Location = new Point(10, 10);
            
            availableVariablesListBox = new ListBox();
            availableVariablesListBox.Location = new Point(10, 35);
            availableVariablesListBox.Size = new Size(300, 120);
            availableVariablesListBox.Font = new Font("Consolas", 9);
            availableVariablesListBox.BackColor = Color.White;
            availableVariablesListBox.BorderStyle = BorderStyle.FixedSingle;
            
            // Populate available variables
            PopulateAvailableVariables();
            
            // Add double-click event handler
            availableVariablesListBox.DoubleClick += AvailableVariables_DoubleClick;
            
            // Template Format section
            Label formatLabel = new Label();
            formatLabel.Text = "Template Format:";
            formatLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            formatLabel.AutoSize = true;
            formatLabel.Location = new Point(10, 170);
            
            formatBox = new TextBox();
            formatBox.Location = new Point(10, 195);
            formatBox.Size = new Size(300, 200);
            formatBox.Multiline = true;
            formatBox.ScrollBars = ScrollBars.Vertical;
            formatBox.Font = new Font("Consolas", 9);
            formatBox.BackColor = Color.White;
            formatBox.BorderStyle = BorderStyle.FixedSingle;
            formatBox.Text = @"Hostname: {hostname}
User: {user}
IP: {ip}
OS: {os}
Cores: {cores}";
            
            rightPanel.Controls.AddRange(new Control[] {
                variablesLabel, availableVariablesListBox, formatLabel, formatBox
            });
        }

        private void PopulateAvailableVariables()
        {
            var variables = new string[]
            {
                "{hostname} - Computer name",
                "{user} - Current user",
                "{ip} - IP address",
                "{os} - Operating system",
                "{cores} - CPU cores"
            };
            
            // Add API variables if any exist
            foreach (var conn in apiConnections)
            {
                variables = variables.Append($"{{API:{conn.Name}}} - {conn.Name} API data").ToArray();
            }
            
            availableVariablesListBox.Items.Clear();
            availableVariablesListBox.Items.AddRange(variables);
        }

        private void AvailableVariables_DoubleClick(object sender, EventArgs e)
        {
            if (availableVariablesListBox.SelectedItem != null)
            {
                string selectedItem = availableVariablesListBox.SelectedItem.ToString();
                // Extract the variable part (before the " - " description)
                string variable = selectedItem.Split(new string[] { " - " }, StringSplitOptions.None)[0];
                
                // Insert the variable at the current cursor position in the format box
                int cursorPosition = formatBox.SelectionStart;
                string currentText = formatBox.Text;
                
                // If there's selected text, replace it; otherwise, insert at cursor
                if (formatBox.SelectionLength > 0)
                {
                    formatBox.Text = currentText.Remove(cursorPosition, formatBox.SelectionLength).Insert(cursorPosition, variable);
                }
                else
                {
                    formatBox.Text = currentText.Insert(cursorPosition, variable);
                }
                
                // Set cursor position after the inserted variable
                formatBox.SelectionStart = cursorPosition + variable.Length;
                formatBox.Focus();
            }
        }

        private Button CreateIconButton(string icon, string tooltip)
        {
            Button btn = new Button
            {
                Width = 28,
                Height = 28,
                Text = icon,
                Font = new Font("Segoe UI Emoji", 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(64, 64, 64),
                Margin = new Padding(2),
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 230, 230);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(210, 210, 210);
            
            toolTip.SetToolTip(btn, tooltip);
            
            return btn;
        }

        private void InitializePreviewArea()
        {
            int previewScalePercent = 50;
            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            int previewW = (screenW * previewScalePercent) / 100;
            int previewH = (screenH * previewScalePercent) / 100;

            previewBox = new PictureBox
            {
                Width = previewW,
                Height = previewH,
                SizeMode = PictureBoxSizeMode.Normal,
                Location = new Point(10, topPanel.Bottom + 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            previewBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var pen = new Pen(Color.LightGray, 1);
                g.DrawRectangle(pen, 0, 0, previewBox.Width - 1, previewBox.Height - 1);
            };

            // Initialize drag panel - ensure it starts at 0,0 relative to previewBox
            dragPanel = new Panel
            {
                BackColor = Color.Transparent,
                Location = new Point(0, 0), // Start at top-left of preview (0,0)
                Size = new Size(500, 100),
                Parent = previewBox,
                Cursor = Cursors.SizeAll
            };
            
            dragPanel.BringToFront();
            dragPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var pen = new Pen(Color.Red, 2);
                g.DrawRectangle(pen, 0, 0, dragPanel.Width - 1, dragPanel.Height - 1);

                // Placeholder text instead of processing variables
                string placeholderText = "Drag me (placeholder)";
                using var brush = new SolidBrush(Color.Red);
                Color background = Color.FromArgb(100, 255, 255, 255);
                e.Graphics.FillRectangle(new SolidBrush(background), 0, 0, dragPanel.Width, dragPanel.Height);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.DrawString(placeholderText, selectedFont, brush, new PointF(0, 0));
            };

            dragPanel.MouseDown += (s, e) => dragOffset = e.Location;
            dragPanel.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    var newLeft = dragPanel.Left + e.X - dragOffset.X;
                    var newTop = dragPanel.Top + e.Y - dragOffset.Y;

                    // Ensure drag panel stays within preview bounds and 0,0 is top-left
                    newLeft = Math.Max(0, Math.Min(newLeft, previewBox.Width - dragPanel.Width));
                    newTop = Math.Max(0, Math.Min(newTop, previewBox.Height - dragPanel.Height));

                    dragPanel.Left = newLeft;
                    dragPanel.Top = newTop;

                    dragPanel.Invalidate();
                    UpdateStatusBar();
                }
            };
        }

        private void InitializeStatusStrip()
        {
            statusStrip = new StatusStrip();
            statusStrip.Dock = DockStyle.Bottom;
            statusStrip.BackColor = Color.FromArgb(240, 240, 240);
            
            previewPosLabel = new ToolStripStatusLabel();
            realPosLabel = new ToolStripStatusLabel();
            
            statusStrip.Items.AddRange(new ToolStripItem[] { previewPosLabel, realPosLabel });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                toolTip?.Dispose();
                updateTimer?.Dispose();
                colorDialog?.Dispose();
                fontDialog?.Dispose();
                trayIcon?.Dispose();
                trayMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
