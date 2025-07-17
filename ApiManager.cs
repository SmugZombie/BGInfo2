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

        private void btnAddOrUpdate_Click(object sender, EventArgs e)
        {
            var conn = new ApiConnection
            {
                Name = txtName.Text,
                Url = txtUrl.Text,
                Method = comboMethod.SelectedItem?.ToString() ?? "GET",
                AuthType = comboAuth.SelectedItem?.ToString() ?? "None",
                Username = txtUsername.Text,
                PasswordOrToken = txtPassword.Text,
                JsonKey = txtJsonKey.Text,
                ContentType = comboFormat.SelectedItem?.ToString() ?? "json",
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

            int timeoutSeconds = 5; // default timeout

            try
            {
                if (!int.TryParse(txtTimeout.Text, out timeoutSeconds))
                {
                    timeoutSeconds = 5;
                }

                string result;

                if (System.IO.File.Exists(url))
                {
                    // If URL is a local file path, read the file content with timeout
                    using var fs = new FileStream(url, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var sr = new StreamReader(fs);
                    var readTask = sr.ReadToEndAsync();
                    if (!readTask.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
                    {
                        MessageBox.Show("File read timed out.", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    result = readTask.Result;
                }
                else if (url.StartsWith("cmd://", StringComparison.OrdinalIgnoreCase))
                {
                    // If URL starts with cmd://, treat the rest as a command to execute with timeout
                    string command = url.Substring(6);
                    var processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + command)
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var process = System.Diagnostics.Process.Start(processInfo);
                    if (!process.WaitForExit(timeoutSeconds * 1000))
                    {
                        process.Kill();
                        MessageBox.Show("Command execution timed out.", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    result = process.StandardOutput.ReadToEnd();
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

                if (contentType == "text" && !string.IsNullOrWhiteSpace(regexPattern))
                {
                    try
                    {
                        var regex = new Regex(regexPattern, RegexOptions.Multiline);
                        var match = regex.Match(result);
                        if (match.Success)
                        {
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

                MessageBox.Show("Extracted Result:\n" + output, "Test Result");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test failed:\n" + ex.Message);
            }
        }

        private void btnTestMatch_Click(object sender, EventArgs e)
        {
            var url = txtUrl.Text;
            var contentType = comboFormat.Text;
            var jsonKey = txtJsonKey.Text;
            var regexPattern = txtRegex.Text;

            try
            {
                string result;

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
                    HttpResponseMessage response = client.GetAsync(url).Result;
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

                MessageBox.Show("Extracted Result:\n" + output, "Test Match Result");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test match failed:\n" + ex.Message);
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
