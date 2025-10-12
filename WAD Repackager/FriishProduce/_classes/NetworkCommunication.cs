using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FriishProduce
{
    /// <summary>
    /// Represents a discovered NXDump device on the network
    /// </summary>
    public class NXDumpDevice
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string DeviceName { get; set; }
        public string Version { get; set; }
        public DateTime LastSeen { get; set; }
        
        public string BaseUrl => $"http://{IPAddress}:{Port}";
    }

    /// <summary>
    /// Represents a game title available on the NXDump device
    /// </summary>
    public class GameTitle
    {
        public string TitleId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public long Size { get; set; }
        public string Type { get; set; } // "Cartridge" or "Digital"
        public bool IsExtractable { get; set; }
    }

    /// <summary>
    /// Represents the status of a file transfer operation
    /// </summary>
    public class TransferStatus
    {
        public string OperationId { get; set; }
        public string Status { get; set; } // "Pending", "InProgress", "Completed", "Failed"
        public long BytesTransferred { get; set; }
        public long TotalBytes { get; set; }
        public string ErrorMessage { get; set; }
        public double ProgressPercentage => TotalBytes > 0 ? (double)BytesTransferred / TotalBytes * 100 : 0;
    }

    /// <summary>
    /// Handles network communication with NXDump devices
    /// </summary>
    public class NetworkCommunication : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly UdpClient discoveryClient;
        private readonly Timer discoveryTimer;
        private readonly List<NXDumpDevice> discoveredDevices;
        private readonly object devicesLock = new object();
        
        private const int DISCOVERY_PORT = 8765;
        private const int DEFAULT_NXDUMP_PORT = 8080;
        private const string DISCOVERY_MESSAGE = "FRIISH_DISCOVER";
        private const string DISCOVERY_RESPONSE_PREFIX = "NXDUMP_RESPONSE";
        
        public event EventHandler<NXDumpDevice> DeviceDiscovered;
        public event EventHandler<NXDumpDevice> DeviceLost;
        public event EventHandler<TransferStatus> TransferProgressUpdated;

        public NetworkCommunication()
        {
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            discoveredDevices = new List<NXDumpDevice>();
            
            try
            {
                discoveryClient = new UdpClient(DISCOVERY_PORT);
                discoveryClient.EnableBroadcast = true;
                
                // Start discovery timer (every 5 seconds)
                discoveryTimer = new Timer(PerformDiscovery, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
                
                // Start listening for discovery responses
                Task.Run(ListenForDiscoveryResponses);
            }
            catch (Exception ex)
            {
                // If we can't bind to the discovery port, continue without auto-discovery
                System.Diagnostics.Debug.WriteLine($"Discovery initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a list of currently discovered NXDump devices
        /// </summary>
        public List<NXDumpDevice> GetDiscoveredDevices()
        {
            lock (devicesLock)
            {
                return new List<NXDumpDevice>(discoveredDevices);
            }
        }

        /// <summary>
        /// Manually add a device by IP address
        /// </summary>
        public async Task<NXDumpDevice> AddDeviceManually(string ipAddress, int port = DEFAULT_NXDUMP_PORT)
        {
            try
            {
                var device = new NXDumpDevice
                {
                    IPAddress = ipAddress,
                    Port = port,
                    LastSeen = DateTime.Now
                };

                // Verify the device is actually an NXDump instance
                var deviceInfo = await GetDeviceInfo(device);
                if (deviceInfo != null)
                {
                    device.DeviceName = deviceInfo.GetValueOrDefault("name", "Unknown Device");
                    device.Version = deviceInfo.GetValueOrDefault("version", "Unknown");
                    
                    lock (devicesLock)
                    {
                        var existing = discoveredDevices.Find(d => d.IPAddress == ipAddress);
                        if (existing != null)
                        {
                            discoveredDevices.Remove(existing);
                        }
                        discoveredDevices.Add(device);
                    }
                    
                    DeviceDiscovered?.Invoke(this, device);
                    return device;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to device at {ipAddress}:{port} - {ex.Message}");
            }
            
            return null;
        }

        /// <summary>
        /// Get basic information about a device
        /// </summary>
        private async Task<Dictionary<string, string>> GetDeviceInfo(NXDumpDevice device)
        {
            try
            {
                var response = await httpClient.GetStringAsync($"{device.BaseUrl}/api/info");
                return JsonSerializer.Deserialize<Dictionary<string, string>>(response);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get list of available game titles from a device
        /// </summary>
        public async Task<List<GameTitle>> GetAvailableTitles(NXDumpDevice device)
        {
            try
            {
                var response = await httpClient.GetStringAsync($"{device.BaseUrl}/api/titles");
                return JsonSerializer.Deserialize<List<GameTitle>>(response);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get titles from device: {ex.Message}");
            }
        }

        /// <summary>
        /// Request extraction of a specific game title
        /// </summary>
        public async Task<string> RequestExtraction(NXDumpDevice device, GameTitle title, string outputFormat = "nsp")
        {
            try
            {
                var requestData = new
                {
                    titleId = title.TitleId,
                    format = outputFormat,
                    includeUpdates = true,
                    includeDLC = false
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync($"{device.BaseUrl}/api/extract", content);
                response.EnsureSuccessStatusCode();
                
                var responseData = JsonSerializer.Deserialize<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                return responseData["operationId"];
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to request extraction: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the status of an extraction operation
        /// </summary>
        public async Task<TransferStatus> GetExtractionStatus(NXDumpDevice device, string operationId)
        {
            try
            {
                var response = await httpClient.GetStringAsync($"{device.BaseUrl}/api/status/{operationId}");
                return JsonSerializer.Deserialize<TransferStatus>(response);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get extraction status: {ex.Message}");
            }
        }

        /// <summary>
        /// Download an extracted file from the device
        /// </summary>
        public async Task<Stream> DownloadExtractedFile(NXDumpDevice device, string operationId, IProgress<long> progress = null)
        {
            try
            {
                var response = await httpClient.GetAsync($"{device.BaseUrl}/api/download/{operationId}", HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                
                var stream = await response.Content.ReadAsStreamAsync();
                
                if (progress != null)
                {
                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    return new ProgressStream(stream, totalBytes, progress);
                }
                
                return stream;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download file: {ex.Message}");
            }
        }

        /// <summary>
        /// Perform network discovery for NXDump devices
        /// </summary>
        private void PerformDiscovery(object state)
        {
            if (discoveryClient == null) return;
            
            try
            {
                var message = Encoding.UTF8.GetBytes(DISCOVERY_MESSAGE);
                var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);
                discoveryClient.Send(message, message.Length, broadcastEndpoint);
                
                // Clean up old devices (not seen for more than 30 seconds)
                lock (devicesLock)
                {
                    var cutoff = DateTime.Now.AddSeconds(-30);
                    var toRemove = discoveredDevices.FindAll(d => d.LastSeen < cutoff);
                    foreach (var device in toRemove)
                    {
                        discoveredDevices.Remove(device);
                        DeviceLost?.Invoke(this, device);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Discovery broadcast failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Listen for discovery responses from NXDump devices
        /// </summary>
        private async Task ListenForDiscoveryResponses()
        {
            if (discoveryClient == null) return;
            
            while (true)
            {
                try
                {
                    var result = await discoveryClient.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    
                    if (message.StartsWith(DISCOVERY_RESPONSE_PREFIX))
                    {
                        var parts = message.Split('|');
                        if (parts.Length >= 4)
                        {
                            var device = new NXDumpDevice
                            {
                                IPAddress = result.RemoteEndPoint.Address.ToString(),
                                Port = int.Parse(parts[1]),
                                DeviceName = parts[2],
                                Version = parts[3],
                                LastSeen = DateTime.Now
                            };
                            
                            lock (devicesLock)
                            {
                                var existing = discoveredDevices.Find(d => d.IPAddress == device.IPAddress);
                                if (existing != null)
                                {
                                    existing.LastSeen = DateTime.Now;
                                }
                                else
                                {
                                    discoveredDevices.Add(device);
                                    DeviceDiscovered?.Invoke(this, device);
                                }
                            }
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Client was disposed, exit the loop
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Discovery listen error: {ex.Message}");
                    await Task.Delay(1000); // Wait before retrying
                }
            }
        }

        public void Dispose()
        {
            discoveryTimer?.Dispose();
            discoveryClient?.Close();
            discoveryClient?.Dispose();
            httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Stream wrapper that reports progress during read operations
    /// </summary>
    public class ProgressStream : Stream
    {
        private readonly Stream baseStream;
        private readonly long totalBytes;
        private readonly IProgress<long> progress;
        private long bytesRead;

        public ProgressStream(Stream baseStream, long totalBytes, IProgress<long> progress)
        {
            this.baseStream = baseStream;
            this.totalBytes = totalBytes;
            this.progress = progress;
        }

        public override bool CanRead => baseStream.CanRead;
        public override bool CanSeek => baseStream.CanSeek;
        public override bool CanWrite => baseStream.CanWrite;
        public override long Length => baseStream.Length;
        public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = baseStream.Read(buffer, offset, count);
            bytesRead += read;
            progress?.Report(bytesRead);
            return read;
        }

        public override void Flush() => baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);
        public override void SetLength(long value) => baseStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => baseStream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                baseStream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}