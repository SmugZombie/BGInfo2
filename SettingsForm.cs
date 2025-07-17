using System;
using System.Windows.Forms;

namespace BgInfoClone
{
    public partial class SettingsForm : Form
    {
        [System.ComponentModel.Browsable(false)]
        public int RefreshIntervalMinutes { get; private set; }

        public SettingsForm(int currentIntervalMinutes)
        {
            InitializeComponent();
            RefreshIntervalMinutes = currentIntervalMinutes;
            
            // Set the current value in the numeric up-down control
            numericUpDownInterval.Value = currentIntervalMinutes;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            RefreshIntervalMinutes = (int)numericUpDownInterval.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
