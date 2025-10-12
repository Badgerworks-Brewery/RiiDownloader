using System;
using System.Net;
using System.Windows.Forms;

namespace FriishProduce
{
    public partial class AddDeviceDialog : Form
    {
        public string IPAddress { get; private set; }
        public int Port { get; private set; }

        private TextBox textBoxIP;
        private NumericUpDown numericUpDownPort;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelIP;
        private Label labelPort;

        public AddDeviceDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.textBoxIP = new TextBox();
            this.numericUpDownPort = new NumericUpDown();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.labelIP = new Label();
            this.labelPort = new Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).BeginInit();
            this.SuspendLayout();
            
            // 
            // labelIP
            // 
            this.labelIP.AutoSize = true;
            this.labelIP.Location = new System.Drawing.Point(12, 15);
            this.labelIP.Name = "labelIP";
            this.labelIP.Size = new System.Drawing.Size(61, 13);
            this.labelIP.TabIndex = 0;
            this.labelIP.Text = "IP Address:";
            
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(79, 12);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(200, 20);
            this.textBoxIP.TabIndex = 1;
            this.textBoxIP.Text = "192.168.1.";
            
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(12, 41);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(29, 13);
            this.labelPort.TabIndex = 2;
            this.labelPort.Text = "Port:";
            
            // 
            // numericUpDownPort
            // 
            this.numericUpDownPort.Location = new System.Drawing.Point(79, 39);
            this.numericUpDownPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numericUpDownPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDownPort.Name = "numericUpDownPort";
            this.numericUpDownPort.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownPort.TabIndex = 3;
            this.numericUpDownPort.Value = new decimal(new int[] { 8080, 0, 0, 0 });
            
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(123, 75);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(204, 75);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            
            // 
            // AddDeviceDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(291, 110);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.numericUpDownPort);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.textBoxIP);
            this.Controls.Add(this.labelIP);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddDeviceDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Add NXDump Device";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            // Validate IP address
            if (!System.Net.IPAddress.TryParse(textBoxIP.Text.Trim(), out _))
            {
                MessageBox.Show("Please enter a valid IP address.", "Invalid IP Address", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIP.Focus();
                return;
            }

            IPAddress = textBoxIP.Text.Trim();
            Port = (int)numericUpDownPort.Value;
        }
    }
}