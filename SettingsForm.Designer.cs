using System;
using System.Drawing;
using System.Windows.Forms;

namespace BgInfoClone
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblInterval;
        private NumericUpDown numericUpDownInterval;
        private Label lblMinutes;
        private Button btnOK;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblInterval = new Label();
            this.numericUpDownInterval = new NumericUpDown();
            this.lblMinutes = new Label();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInterval)).BeginInit();
            this.SuspendLayout();

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblTitle.Location = new Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(78, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Settings";

            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Font = new Font("Segoe UI", 9F);
            this.lblInterval.Location = new Point(20, 60);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new Size(140, 15);
            this.lblInterval.TabIndex = 1;
            this.lblInterval.Text = "Auto-refresh interval:";

            // 
            // numericUpDownInterval
            // 
            this.numericUpDownInterval.Font = new Font("Segoe UI", 9F);
            this.numericUpDownInterval.Location = new Point(20, 85);
            this.numericUpDownInterval.Maximum = new decimal(new int[] { 1440, 0, 0, 0 }); // Max 24 hours
            this.numericUpDownInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 }); // Min 1 minute
            this.numericUpDownInterval.Name = "numericUpDownInterval";
            this.numericUpDownInterval.Size = new Size(80, 23);
            this.numericUpDownInterval.TabIndex = 2;
            this.numericUpDownInterval.Value = new decimal(new int[] { 30, 0, 0, 0 }); // Default 30 minutes

            // 
            // lblMinutes
            // 
            this.lblMinutes.AutoSize = true;
            this.lblMinutes.Font = new Font("Segoe UI", 9F);
            this.lblMinutes.Location = new Point(110, 87);
            this.lblMinutes.Name = "lblMinutes";
            this.lblMinutes.Size = new Size(51, 15);
            this.lblMinutes.TabIndex = 3;
            this.lblMinutes.Text = "minutes";

            // 
            // btnOK
            // 
            this.btnOK.Font = new Font("Segoe UI", 9F);
            this.btnOK.Location = new Point(120, 130);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(75, 30);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);

            // 
            // btnCancel
            // 
            this.btnCancel.Font = new Font("Segoe UI", 9F);
            this.btnCancel.Location = new Point(205, 130);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(75, 30);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(300, 180);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblMinutes);
            this.Controls.Add(this.numericUpDownInterval);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
