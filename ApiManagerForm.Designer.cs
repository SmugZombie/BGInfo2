namespace BgInfoClone
{
    partial class ApiManagerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.listApis = new System.Windows.Forms.ListBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtJsonKey = new System.Windows.Forms.TextBox();
            this.txtRegex = new System.Windows.Forms.TextBox();
            this.comboMethod = new System.Windows.Forms.ComboBox();
            this.comboAuth = new System.Windows.Forms.ComboBox();
            this.comboFormat = new System.Windows.Forms.ComboBox();
            this.btnAddOrUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnTestMatch = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // listApis
            //
            this.listApis.FormattingEnabled = true;
            this.listApis.ItemHeight = 15;
            this.listApis.Location = new System.Drawing.Point(12, 12);
            this.listApis.Size = new System.Drawing.Size(200, 304);
            this.listApis.SelectedIndexChanged += new System.EventHandler(this.listApis_SelectedIndexChanged);
            //
            // txtName
            //
            this.txtName.Location = new System.Drawing.Point(230, 12);
            this.txtName.PlaceholderText = "Connection Name";
            this.txtName.Size = new System.Drawing.Size(300, 23);
            //
            // txtUrl
            //
            this.txtUrl.Location = new System.Drawing.Point(230, 41);
            this.txtUrl.PlaceholderText = "Endpoint URL";
            this.txtUrl.Size = new System.Drawing.Size(300, 23);
            //
            // comboMethod
            //
            this.comboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMethod.Items.AddRange(new object[] { "GET", "POST" });
            this.comboMethod.Location = new System.Drawing.Point(230, 70);
            this.comboMethod.Size = new System.Drawing.Size(100, 23);
            //
            // comboAuth
            //
            this.comboAuth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAuth.Items.AddRange(new object[] { "None", "Basic", "Bearer" });
            this.comboAuth.Location = new System.Drawing.Point(340, 70);
            this.comboAuth.Size = new System.Drawing.Size(100, 23);
            //
            // comboFormat
            //
            this.comboFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFormat.Items.AddRange(new object[] { "json", "text" });
            this.comboFormat.Location = new System.Drawing.Point(450, 70);
            this.comboFormat.Size = new System.Drawing.Size(80, 23);
            //
            // txtUsername
            //
            this.txtUsername.Location = new System.Drawing.Point(230, 99);
            this.txtUsername.PlaceholderText = "Username";
            this.txtUsername.Size = new System.Drawing.Size(300, 23);
            //
            // txtPassword
            //
            this.txtPassword.Location = new System.Drawing.Point(230, 128);
            this.txtPassword.PlaceholderText = "Password or Token";
            this.txtPassword.Size = new System.Drawing.Size(300, 23);
            //
            // txtJsonKey
            //
            this.txtJsonKey.Location = new System.Drawing.Point(230, 157);
            this.txtJsonKey.PlaceholderText = "JSON Key (dot.notation)";
            this.txtJsonKey.Size = new System.Drawing.Size(300, 23);
            //
            // txtRegex
            //
            this.txtRegex.Location = new System.Drawing.Point(230, 186);
            this.txtRegex.PlaceholderText = "Regex Pattern (for text APIs)";
            this.txtRegex.Size = new System.Drawing.Size(300, 23);
            //
            // btnAddOrUpdate
            //
            this.btnAddOrUpdate.Location = new System.Drawing.Point(230, 215);
            this.btnAddOrUpdate.Size = new System.Drawing.Size(90, 25);
            this.btnAddOrUpdate.Text = "Add / Update";
            this.btnAddOrUpdate.Click += new System.EventHandler(this.btnAddOrUpdate_Click);
            //
            // btnDelete
            //
            this.btnDelete.Location = new System.Drawing.Point(330, 215);
            this.btnDelete.Size = new System.Drawing.Size(60, 25);
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            //
            // btnTest
            //
            this.btnTest.Location = new System.Drawing.Point(400, 215);
            this.btnTest.Size = new System.Drawing.Size(80, 25);
            this.btnTest.Text = "Test Endpoint";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            //
            // btnTestMatch
            //
            this.btnTestMatch.Location = new System.Drawing.Point(485, 215);
            this.btnTestMatch.Size = new System.Drawing.Size(80, 25);
            this.btnTestMatch.Text = "Test Match";
            this.btnTestMatch.Click += new System.EventHandler(this.btnTestMatch_Click);
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(575, 215);
            this.btnClose.Size = new System.Drawing.Size(60, 25);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // ApiManagerForm
            //
            this.ClientSize = new System.Drawing.Size(660, 330);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.listApis, this.txtName, this.txtUrl, this.comboMethod, this.comboAuth, this.comboFormat,
                this.txtUsername, this.txtPassword, this.txtJsonKey, this.txtRegex,
                this.btnAddOrUpdate, this.btnDelete, this.btnTest, this.btnTestMatch, this.btnClose });
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Text = "API Manager";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ListBox listApis;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtJsonKey;
        private System.Windows.Forms.TextBox txtRegex;
        private System.Windows.Forms.ComboBox comboMethod;
        private System.Windows.Forms.ComboBox comboAuth;
        private System.Windows.Forms.ComboBox comboFormat;
        private System.Windows.Forms.Button btnAddOrUpdate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnTestMatch;
        private System.Windows.Forms.Button btnClose;
    }
}
