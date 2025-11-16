namespace FriishProduce
{
    partial class NetworkIntegrationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                romInjector?.Dispose();
                networkComm?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxDevices = new System.Windows.Forms.GroupBox();
            this.listViewDevices = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonAddManually = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.groupBoxTitles = new System.Windows.Forms.GroupBox();
            this.listViewTitles = new System.Windows.Forms.ListView();
            this.columnHeaderTitleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTitleId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRefreshTitles = new System.Windows.Forms.Button();
            this.groupBoxConsole = new System.Windows.Forms.GroupBox();
            this.comboBoxConsole = new System.Windows.Forms.ComboBox();
            this.labelConsole = new System.Windows.Forms.Label();
            this.buttonExtractAndInject = new System.Windows.Forms.Button();
            this.groupBoxProgress = new System.Windows.Forms.GroupBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.groupBoxDevices.SuspendLayout();
            this.groupBoxTitles.SuspendLayout();
            this.groupBoxConsole.SuspendLayout();
            this.groupBoxProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxDevices
            // 
            this.groupBoxDevices.Controls.Add(this.listViewDevices);
            this.groupBoxDevices.Controls.Add(this.buttonRefresh);
            this.groupBoxDevices.Controls.Add(this.buttonAddManually);
            this.groupBoxDevices.Controls.Add(this.buttonConnect);
            this.groupBoxDevices.Location = new System.Drawing.Point(12, 12);
            this.groupBoxDevices.Name = "groupBoxDevices";
            this.groupBoxDevices.Size = new System.Drawing.Size(760, 200);
            this.groupBoxDevices.TabIndex = 0;
            this.groupBoxDevices.TabStop = false;
            this.groupBoxDevices.Text = "NXDump Devices";
            // 
            // listViewDevices
            // 
            this.listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderIP,
            this.columnHeaderVersion,
            this.columnHeaderStatus});
            this.listViewDevices.FullRowSelect = true;
            this.listViewDevices.GridLines = true;
            this.listViewDevices.Location = new System.Drawing.Point(6, 19);
            this.listViewDevices.MultiSelect = false;
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.Size = new System.Drawing.Size(748, 140);
            this.listViewDevices.TabIndex = 0;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            this.listViewDevices.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Device Name";
            this.columnHeaderName.Width = 200;
            // 
            // columnHeaderIP
            // 
            this.columnHeaderIP.Text = "IP Address";
            this.columnHeaderIP.Width = 150;
            // 
            // columnHeaderVersion
            // 
            this.columnHeaderVersion.Text = "Version";
            this.columnHeaderVersion.Width = 100;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 298;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(6, 165);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 1;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            // 
            // buttonAddManually
            // 
            this.buttonAddManually.Location = new System.Drawing.Point(87, 165);
            this.buttonAddManually.Name = "buttonAddManually";
            this.buttonAddManually.Size = new System.Drawing.Size(100, 23);
            this.buttonAddManually.TabIndex = 2;
            this.buttonAddManually.Text = "Add Manually...";
            this.buttonAddManually.UseVisualStyleBackColor = true;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Enabled = false;
            this.buttonConnect.Location = new System.Drawing.Point(679, 165);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 3;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            // 
            // groupBoxTitles
            // 
            this.groupBoxTitles.Controls.Add(this.listViewTitles);
            this.groupBoxTitles.Controls.Add(this.buttonRefreshTitles);
            this.groupBoxTitles.Enabled = false;
            this.groupBoxTitles.Location = new System.Drawing.Point(12, 218);
            this.groupBoxTitles.Name = "groupBoxTitles";
            this.groupBoxTitles.Size = new System.Drawing.Size(760, 200);
            this.groupBoxTitles.TabIndex = 1;
            this.groupBoxTitles.TabStop = false;
            this.groupBoxTitles.Text = "Available Titles";
            // 
            // listViewTitles
            // 
            this.listViewTitles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderTitleName,
            this.columnHeaderTitleId,
            this.columnHeaderSize,
            this.columnHeaderType});
            this.listViewTitles.FullRowSelect = true;
            this.listViewTitles.GridLines = true;
            this.listViewTitles.Location = new System.Drawing.Point(6, 19);
            this.listViewTitles.MultiSelect = false;
            this.listViewTitles.Name = "listViewTitles";
            this.listViewTitles.Size = new System.Drawing.Size(748, 140);
            this.listViewTitles.TabIndex = 0;
            this.listViewTitles.UseCompatibleStateImageBehavior = false;
            this.listViewTitles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderTitleName
            // 
            this.columnHeaderTitleName.Text = "Title Name";
            this.columnHeaderTitleName.Width = 300;
            // 
            // columnHeaderTitleId
            // 
            this.columnHeaderTitleId.Text = "Title ID";
            this.columnHeaderTitleId.Width = 150;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 100;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Type";
            this.columnHeaderType.Width = 198;
            // 
            // buttonRefreshTitles
            // 
            this.buttonRefreshTitles.Location = new System.Drawing.Point(6, 165);
            this.buttonRefreshTitles.Name = "buttonRefreshTitles";
            this.buttonRefreshTitles.Size = new System.Drawing.Size(100, 23);
            this.buttonRefreshTitles.TabIndex = 1;
            this.buttonRefreshTitles.Text = "Refresh Titles";
            this.buttonRefreshTitles.UseVisualStyleBackColor = true;
            // 
            // groupBoxConsole
            // 
            this.groupBoxConsole.Controls.Add(this.comboBoxConsole);
            this.groupBoxConsole.Controls.Add(this.labelConsole);
            this.groupBoxConsole.Controls.Add(this.buttonExtractAndInject);
            this.groupBoxConsole.Enabled = false;
            this.groupBoxConsole.Location = new System.Drawing.Point(12, 424);
            this.groupBoxConsole.Name = "groupBoxConsole";
            this.groupBoxConsole.Size = new System.Drawing.Size(760, 60);
            this.groupBoxConsole.TabIndex = 2;
            this.groupBoxConsole.TabStop = false;
            this.groupBoxConsole.Text = "Target Console";
            // 
            // comboBoxConsole
            // 
            this.comboBoxConsole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxConsole.FormattingEnabled = true;
            this.comboBoxConsole.Location = new System.Drawing.Point(100, 25);
            this.comboBoxConsole.Name = "comboBoxConsole";
            this.comboBoxConsole.Size = new System.Drawing.Size(200, 21);
            this.comboBoxConsole.TabIndex = 1;
            // 
            // labelConsole
            // 
            this.labelConsole.AutoSize = true;
            this.labelConsole.Location = new System.Drawing.Point(6, 28);
            this.labelConsole.Name = "labelConsole";
            this.labelConsole.Size = new System.Drawing.Size(88, 13);
            this.labelConsole.TabIndex = 0;
            this.labelConsole.Text = "Target Console:";
            // 
            // buttonExtractAndInject
            // 
            this.buttonExtractAndInject.Enabled = false;
            this.buttonExtractAndInject.Location = new System.Drawing.Point(600, 23);
            this.buttonExtractAndInject.Name = "buttonExtractAndInject";
            this.buttonExtractAndInject.Size = new System.Drawing.Size(154, 25);
            this.buttonExtractAndInject.TabIndex = 2;
            this.buttonExtractAndInject.Text = "Extract && Create WAD";
            this.buttonExtractAndInject.UseVisualStyleBackColor = true;
            // 
            // groupBoxProgress
            // 
            this.groupBoxProgress.Controls.Add(this.progressBar);
            this.groupBoxProgress.Controls.Add(this.labelStatus);
            this.groupBoxProgress.Controls.Add(this.buttonCancel);
            this.groupBoxProgress.Location = new System.Drawing.Point(12, 490);
            this.groupBoxProgress.Name = "groupBoxProgress";
            this.groupBoxProgress.Size = new System.Drawing.Size(760, 80);
            this.groupBoxProgress.TabIndex = 3;
            this.groupBoxProgress.TabStop = false;
            this.groupBoxProgress.Text = "Progress";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(6, 19);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(748, 23);
            this.progressBar.TabIndex = 0;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(6, 50);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(37, 13);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "Ready";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Enabled = false;
            this.buttonCancel.Location = new System.Drawing.Point(679, 45);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "wad";
            this.saveFileDialog.Filter = "WAD Files (*.wad)|*.wad|All Files (*.*)|*.*";
            this.saveFileDialog.Title = "Save WAD File";
            // 
            // NetworkIntegrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 582);
            this.Controls.Add(this.groupBoxProgress);
            this.Controls.Add(this.groupBoxConsole);
            this.Controls.Add(this.groupBoxTitles);
            this.Controls.Add(this.groupBoxDevices);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetworkIntegrationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NXDump Network Integration";
            this.groupBoxDevices.ResumeLayout(false);
            this.groupBoxTitles.ResumeLayout(false);
            this.groupBoxConsole.ResumeLayout(false);
            this.groupBoxConsole.PerformLayout();
            this.groupBoxProgress.ResumeLayout(false);
            this.groupBoxProgress.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxDevices;
        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderIP;
        private System.Windows.Forms.ColumnHeader columnHeaderVersion;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonAddManually;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.GroupBox groupBoxTitles;
        private System.Windows.Forms.ListView listViewTitles;
        private System.Windows.Forms.ColumnHeader columnHeaderTitleName;
        private System.Windows.Forms.ColumnHeader columnHeaderTitleId;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.Button buttonRefreshTitles;
        private System.Windows.Forms.GroupBox groupBoxConsole;
        private System.Windows.Forms.ComboBox comboBoxConsole;
        private System.Windows.Forms.Label labelConsole;
        private System.Windows.Forms.Button buttonExtractAndInject;
        private System.Windows.Forms.GroupBox groupBoxProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}