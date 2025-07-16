using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BgInfoClone
{
    public partial class ApiManagerForm : Form
    {
        private List<ApiConnection> connections;
        private int editingIndex = -1;

        public List<ApiConnection> UpdatedConnections => connections;

        public ApiManagerForm(List<ApiConnection> existingConnections)
        {
            InitializeComponent();
            connections = new List<ApiConnection>(existingConnections);
            PopulateList();
            comboMethod.SelectedIndex = 0;
            comboAuth.SelectedIndex = 0;
            comboFormat.SelectedIndex = 0;
        }

        private void PopulateList()
        {
            listApis.Items.Clear();
            foreach (var conn in connections)
            {
                listApis.Items.Add(conn.Name);
            }
        }

        private void btnAddOrUpdate_Click(object sender, EventArgs e)
        {
            var conn = new ApiConnection
            {
                Name = txtName.Text,
                Url = txtUrl.Text,
                Method = comboMethod.Text,
                AuthType = comboAuth.Text,
                Username = txtUsername.Text,
                PasswordOrToken = txtPassword.Text,
                JsonKey = txtJsonKey.Text,
                ContentType = comboFormat.Text,
                RegexPattern = txtRegex.Text
            };

            if (editingIndex >= 0)
            {
                connections[editingIndex] = conn;
                editingIndex = -1;
            }
            else
            {
                connections.Add(conn);
            }

            ClearFields();
            PopulateList();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listApis.SelectedIndex >= 0)
            {
                connections.RemoveAt(listApis.SelectedIndex);
                PopulateList();
            }
        }

        private void listApis_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listApis.SelectedIndex >= 0)
            {
                var conn = connections[listApis.SelectedIndex];
                txtName.Text = conn.Name;
                txtUrl.Text = conn.Url;
                comboMethod.Text = conn.Method;
                comboAuth.Text = conn.AuthType;
                txtUsername.Text = conn.Username;
                txtPassword.Text = conn.PasswordOrToken;
                txtJsonKey.Text = conn.JsonKey;
                comboFormat.Text = conn.ContentType;
                txtRegex.Text = conn.RegexPattern;
                editingIndex = listApis.SelectedIndex;
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var url = txtUrl.Text;
            var method = comboMethod.Text.ToUpper();
            var authType = comboAuth.Text;
            var username = txtUsername.Text;
            var passwordOrToken = txtPassword.Text;
            var contentType = comboFormat.Text;
            var regexPattern = txtRegex.Text;

            try
            {
                using var client = new HttpClient();

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

                string result = response.Content.ReadAsStringAsync().Result;
                string output = result;

                if (contentType == "text" && !string.IsNullOrWhiteSpace(regexPattern))
                {
                    var match = Regex.Match(result, regexPattern);
                    if (match.Success)
                    {
                        output = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                    }
                    else
                    {
                        output = "(no match)";
                    }
                }

                MessageBox.Show("Extracted Result:\n" + output, "Test Result");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test failed:\n" + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ClearFields()
        {
            txtName.Text = txtUrl.Text = txtUsername.Text = txtPassword.Text = txtJsonKey.Text = txtRegex.Text = "";
            comboMethod.SelectedIndex = 0;
            comboAuth.SelectedIndex = 0;
            comboFormat.SelectedIndex = 0;
        }
    }

    public class ApiConnection
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Method { get; set; } = "GET";
        public string AuthType { get; set; } = "None";
        public string Username { get; set; } = "";
        public string PasswordOrToken { get; set; } = "";
        public string JsonKey { get; set; } = "";
        public string ContentType { get; set; } = "json";
        public string RegexPattern { get; set; } = "";
    }
}
