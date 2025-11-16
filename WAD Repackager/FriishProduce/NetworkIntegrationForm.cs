using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FriishProduce
{
    public partial class NetworkIntegrationForm : Form
    {
        private NetworkCommunication networkComm;
        private NetworkRomInjector romInjector;
        private NXDumpDevice selectedDevice;
        private GameTitle selectedTitle;
        private bool isOperationInProgress;

        public NetworkIntegrationForm()
        {
            InitializeComponent();
            InitializeNetworking();
            InitializeUI();
        }

        private void InitializeNetworking()
        {
            try
            {
                networkComm = new NetworkCommunication();
                networkComm.DeviceDiscovered += OnDeviceDiscovered;
                networkComm.DeviceLost += OnDeviceLost;

                romInjector = new NetworkRomInjector(networkComm);
                romInjector.StatusUpdated += OnStatusUpdated;
                romInjector.ProgressUpdated += OnProgressUpdated;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize network communication: {ex.Message}", 
                    "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeUI()
        {
            // Populate console dropdown
            comboBoxConsole.Items.Clear();
            comboBoxConsole.Items.Add(new ConsoleItem(Console.NES, "Nintendo Entertainment System (NES)"));
            comboBoxConsole.Items.Add(new ConsoleItem(Console.SNES, "Super Nintendo Entertainment System (SNES)"));
            comboBoxConsole.Items.Add(new ConsoleItem(Console.N64, "Nintendo 64"));
            comboBoxConsole.Items.Add(new ConsoleItem(Console.SMDGEN, "SEGA Genesis / Mega Drive"));
            comboBoxConsole.SelectedIndex = 0;

            // Wire up event handlers
            buttonRefresh.Click += ButtonRefresh_Click;
            buttonAddManually.Click += ButtonAddManually_Click;
            buttonConnect.Click += ButtonConnect_Click;
            buttonRefreshTitles.Click += ButtonRefreshTitles_Click;
            buttonExtractAndInject.Click += ButtonExtractAndInject_Click;
            buttonCancel.Click += ButtonCancel_Click;

            listViewDevices.SelectedIndexChanged += ListViewDevices_SelectedIndexChanged;
            listViewTitles.SelectedIndexChanged += ListViewTitles_SelectedIndexChanged;
            comboBoxConsole.SelectedIndexChanged += ComboBoxConsole_SelectedIndexChanged;

            // Initial refresh
            RefreshDeviceList();
        }

        private void OnDeviceDiscovered(object sender, NXDumpDevice device)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnDeviceDiscovered(sender, device)));
                return;
            }

            AddOrUpdateDeviceInList(device);
        }

        private void OnDeviceLost(object sender, NXDumpDevice device)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnDeviceLost(sender, device)));
                return;
            }

            RemoveDeviceFromList(device);
        }

        private void OnStatusUpdated(object sender, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnStatusUpdated(sender, status)));
                return;
            }

            labelStatus.Text = status;
        }

        private void OnProgressUpdated(object sender, int progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnProgressUpdated(sender, progress)));
                return;
            }

            progressBar.Value = Math.Max(0, Math.Min(100, progress));
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            RefreshDeviceList();
        }

        private async void ButtonAddManually_Click(object sender, EventArgs e)
        {
            using (var dialog = new AddDeviceDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        buttonAddManually.Enabled = false;
                        labelStatus.Text = "Connecting to device...";

                        var device = await networkComm.AddDeviceManually(dialog.IPAddress, dialog.Port);
                        if (device != null)
                        {
                            labelStatus.Text = $"Successfully added device: {device.DeviceName}";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to add device: {ex.Message}", 
                            "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        labelStatus.Text = "Failed to add device";
                    }
                    finally
                    {
                        buttonAddManually.Enabled = true;
                    }
                }
            }
        }

        private async void ButtonConnect_Click(object sender, EventArgs e)
        {
            if (selectedDevice == null)
            {
                return;
            }

            try
            {
                buttonConnect.Enabled = false;
                labelStatus.Text = "Connecting to device...";

                var success = await romInjector.ConnectToDevice(selectedDevice);
                if (success)
                {
                    groupBoxTitles.Enabled = true;
                    await RefreshTitlesList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to device: {ex.Message}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonConnect.Enabled = true;
            }
        }

        private async void ButtonRefreshTitles_Click(object sender, EventArgs e)
        {
            await RefreshTitlesList();
        }

        private async void ButtonExtractAndInject_Click(object sender, EventArgs e)
        {
            if (selectedTitle == null || comboBoxConsole.SelectedItem == null)
            {
                return;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    isOperationInProgress = true;
                    SetUIEnabled(false);
                    buttonCancel.Enabled = true;

                    var consoleItem = (ConsoleItem)comboBoxConsole.SelectedItem;
                    var success = await romInjector.ExtractAndInjectRom(selectedTitle, consoleItem.Console, saveFileDialog.FileName);

                    if (success)
                    {
                        MessageBox.Show("WAD file created successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to create WAD file. Check the status for details.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Operation failed: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    isOperationInProgress = false;
                    SetUIEnabled(true);
                    buttonCancel.Enabled = false;
                    progressBar.Value = 0;
                }
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            // TODO: Implement cancellation logic
            MessageBox.Show("Cancellation is not yet implemented.", "Information", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ListViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count > 0)
            {
                selectedDevice = (NXDumpDevice)listViewDevices.SelectedItems[0].Tag;
                buttonConnect.Enabled = !isOperationInProgress;
            }
            else
            {
                selectedDevice = null;
                buttonConnect.Enabled = false;
            }
        }

        private void ListViewTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewTitles.SelectedItems.Count > 0)
            {
                selectedTitle = (GameTitle)listViewTitles.SelectedItems[0].Tag;
                UpdateExtractButtonState();
            }
            else
            {
                selectedTitle = null;
                buttonExtractAndInject.Enabled = false;
            }
        }

        private void ComboBoxConsole_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateExtractButtonState();
        }

        private void UpdateExtractButtonState()
        {
            buttonExtractAndInject.Enabled = !isOperationInProgress && 
                selectedTitle != null && 
                comboBoxConsole.SelectedItem != null &&
                groupBoxTitles.Enabled;
        }

        private void RefreshDeviceList()
        {
            listViewDevices.Items.Clear();
            var devices = networkComm.GetDiscoveredDevices();
            
            foreach (var device in devices)
            {
                AddOrUpdateDeviceInList(device);
            }

            labelStatus.Text = $"Found {devices.Count} device(s)";
        }

        private void AddOrUpdateDeviceInList(NXDumpDevice device)
        {
            var existingItem = listViewDevices.Items.Cast<ListViewItem>()
                .FirstOrDefault(item => ((NXDumpDevice)item.Tag).IPAddress == device.IPAddress);

            if (existingItem != null)
            {
                // Update existing item
                existingItem.SubItems[0].Text = device.DeviceName ?? "Unknown Device";
                existingItem.SubItems[1].Text = device.IPAddress;
                existingItem.SubItems[2].Text = device.Version ?? "Unknown";
                existingItem.SubItems[3].Text = "Available";
                existingItem.Tag = device;
            }
            else
            {
                // Add new item
                var item = new ListViewItem(device.DeviceName ?? "Unknown Device");
                item.SubItems.Add(device.IPAddress);
                item.SubItems.Add(device.Version ?? "Unknown");
                item.SubItems.Add("Available");
                item.Tag = device;
                listViewDevices.Items.Add(item);
            }
        }

        private void RemoveDeviceFromList(NXDumpDevice device)
        {
            var itemToRemove = listViewDevices.Items.Cast<ListViewItem>()
                .FirstOrDefault(item => ((NXDumpDevice)item.Tag).IPAddress == device.IPAddress);

            if (itemToRemove != null)
            {
                listViewDevices.Items.Remove(itemToRemove);
            }
        }

        private async Task RefreshTitlesList()
        {
            if (selectedDevice == null)
            {
                return;
            }

            try
            {
                buttonRefreshTitles.Enabled = false;
                labelStatus.Text = "Loading titles...";

                var titles = await romInjector.GetAvailableTitles();
                
                listViewTitles.Items.Clear();
                foreach (var title in titles)
                {
                    var item = new ListViewItem(title.Name);
                    item.SubItems.Add(title.TitleId);
                    item.SubItems.Add(FormatFileSize(title.Size));
                    item.SubItems.Add(title.Type);
                    item.Tag = title;
                    listViewTitles.Items.Add(item);
                }

                labelStatus.Text = $"Loaded {titles.Count} title(s)";
                groupBoxConsole.Enabled = titles.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load titles: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                labelStatus.Text = "Failed to load titles";
            }
            finally
            {
                buttonRefreshTitles.Enabled = true;
            }
        }

        private void SetUIEnabled(bool enabled)
        {
            groupBoxDevices.Enabled = enabled;
            groupBoxTitles.Enabled = enabled && selectedDevice != null;
            groupBoxConsole.Enabled = enabled && selectedTitle != null;
            if (enabled)
            {
                UpdateExtractButtonState();
            }
            else
            {
                buttonExtractAndInject.Enabled = false;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Helper class for console dropdown items
    /// </summary>
    public class ConsoleItem
    {
        public Console Console { get; }
        public string DisplayName { get; }

        public ConsoleItem(Console console, string displayName)
        {
            Console = console;
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}