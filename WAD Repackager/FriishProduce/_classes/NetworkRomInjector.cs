using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using libWiiSharp;

namespace FriishProduce
{
    /// <summary>
    /// Enhanced ROM injector that can work with network-sourced files from NXDump
    /// </summary>
    public class NetworkRomInjector : InjectorWiiVC
    {
        private readonly NetworkCommunication networkComm;
        private NXDumpDevice connectedDevice;
        private string currentOperationId;
        
        public event EventHandler<string> StatusUpdated;
        public event EventHandler<int> ProgressUpdated;

        public NetworkRomInjector(NetworkCommunication networkComm) : base()
        {
            this.networkComm = networkComm;
            this.networkComm.TransferProgressUpdated += OnTransferProgressUpdated;
        }

        /// <summary>
        /// Connect to a specific NXDump device
        /// </summary>
        public async Task<bool> ConnectToDevice(NXDumpDevice device)
        {
            try
            {
                // Test connection by getting device info
                var titles = await networkComm.GetAvailableTitles(device);
                connectedDevice = device;
                StatusUpdated?.Invoke(this, $"Connected to {device.DeviceName} at {device.IPAddress}");
                return true;
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke(this, $"Failed to connect to device: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get available titles from the connected device
        /// </summary>
        public async Task<List<GameTitle>> GetAvailableTitles()
        {
            if (connectedDevice == null)
                throw new InvalidOperationException("No device connected");

            return await networkComm.GetAvailableTitles(connectedDevice);
        }

        /// <summary>
        /// Extract and inject a ROM from the connected NXDump device
        /// </summary>
        public async Task<bool> ExtractAndInjectRom(GameTitle selectedTitle, Console targetConsole, string outputPath)
        {
            if (connectedDevice == null)
                throw new InvalidOperationException("No device connected");

            try
            {
                StatusUpdated?.Invoke(this, "Requesting ROM extraction from Switch...");
                
                // Request extraction from NXDump
                currentOperationId = await networkComm.RequestExtraction(connectedDevice, selectedTitle, "nsp");
                
                StatusUpdated?.Invoke(this, "Extraction started, waiting for completion...");
                
                // Poll for completion
                TransferStatus status;
                do
                {
                    await Task.Delay(2000); // Wait 2 seconds between polls
                    status = await networkComm.GetExtractionStatus(connectedDevice, currentOperationId);
                    
                    if (status.Status == "InProgress")
                    {
                        ProgressUpdated?.Invoke(this, (int)status.ProgressPercentage);
                        StatusUpdated?.Invoke(this, $"Extracting... {status.ProgressPercentage:F1}%");
                    }
                    else if (status.Status == "Failed")
                    {
                        throw new Exception($"Extraction failed: {status.ErrorMessage}");
                    }
                } while (status.Status != "Completed");

                StatusUpdated?.Invoke(this, "Extraction completed, downloading file...");
                
                // Download the extracted file
                using (var downloadStream = await networkComm.DownloadExtractedFile(connectedDevice, currentOperationId, 
                    new Progress<long>(bytes => ProgressUpdated?.Invoke(this, (int)((double)bytes / status.TotalBytes * 100)))))
                {
                    StatusUpdated?.Invoke(this, "Processing extracted ROM...");
                    
                    // Process the downloaded NSP file to extract the ROM
                    var romData = await ExtractRomFromNsp(downloadStream, targetConsole);
                    
                    if (romData != null)
                    {
                        StatusUpdated?.Invoke(this, "Creating WAD file...");
                        
                        // Create temporary file for the ROM
                        var tempRomPath = Path.GetTempFileName();
                        File.WriteAllBytes(tempRomPath, romData);
                        
                        try
                        {
                            // Use the appropriate injector based on console type
                            var success = await InjectRomToWad(tempRomPath, targetConsole, outputPath, selectedTitle);
                            
                            if (success)
                            {
                                StatusUpdated?.Invoke(this, "WAD creation completed successfully!");
                                return true;
                            }
                        }
                        finally
                        {
                            // Clean up temporary file
                            if (File.Exists(tempRomPath))
                                File.Delete(tempRomPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
            
            return false;
        }

        /// <summary>
        /// Extract ROM data from NSP file stream
        /// </summary>
        private async Task<byte[]> ExtractRomFromNsp(Stream nspStream, Console targetConsole)
        {
            try
            {
                // Create temporary file for NSP processing
                var tempNspPath = Path.GetTempFileName();
                
                try
                {
                    // Save NSP to temporary file
                    using (var fileStream = File.Create(tempNspPath))
                    {
                        await nspStream.CopyToAsync(fileStream);
                    }
                    
                    // Process NSP based on target console
                    return await ProcessNspForConsole(tempNspPath, targetConsole);
                }
                finally
                {
                    if (File.Exists(tempNspPath))
                        File.Delete(tempNspPath);
                }
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke(this, $"Failed to extract ROM from NSP: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Process NSP file to extract ROM for specific console
        /// </summary>
        private async Task<byte[]> ProcessNspForConsole(string nspPath, Console targetConsole)
        {
            // This is a simplified implementation - in reality, you'd need to:
            // 1. Parse the NSP file structure
            // 2. Extract the appropriate ROM file based on the target console
            // 3. Handle different ROM formats (NES, SNES, N64, etc.)
            
            StatusUpdated?.Invoke(this, $"Processing NSP for {targetConsole} console...");
            
            // For now, we'll simulate ROM extraction
            // In a real implementation, you would:
            // - Use NSP parsing libraries
            // - Extract specific files based on console type
            // - Convert formats if necessary
            
            await Task.Delay(1000); // Simulate processing time
            
            // Return dummy ROM data for demonstration
            // Replace this with actual NSP processing logic
            var dummyRom = new byte[1024 * 1024]; // 1MB dummy ROM
            new Random().NextBytes(dummyRom);
            
            StatusUpdated?.Invoke(this, "ROM extraction from NSP completed");
            return dummyRom;
        }

        /// <summary>
        /// Inject ROM into WAD using appropriate injector
        /// </summary>
        private async Task<bool> InjectRomToWad(string romPath, Console targetConsole, string outputPath, GameTitle gameInfo)
        {
            try
            {
                InjectorWiiVC injector = null;
                
                // Create appropriate injector based on console type
                switch (targetConsole)
                {
                    case Console.NES:
                        injector = new Injectors.NES();
                        break;
                    case Console.SNES:
                        injector = new Injectors.SNES();
                        break;
                    case Console.N64:
                        injector = new Injectors.N64();
                        break;
                    case Console.SMDGEN:
                        injector = new Injectors.SEGA();
                        break;
                    default:
                        throw new NotSupportedException($"Console {targetConsole} is not supported yet");
                }
                
                if (injector != null)
                {
                    // Configure injector with ROM file
                    injector.ROM = File.ReadAllBytes(romPath);
                    
                    // Set up WAD creation parameters
                    var creator = new Creator(targetConsole);
                    creator.TitleID = GenerateTitleId(gameInfo);
                    creator.ChannelTitles = new[] { gameInfo.Name, gameInfo.Name, gameInfo.Name, gameInfo.Name };
                    creator.BannerTitle = gameInfo.Name;
                    creator.BannerYear = DateTime.Now.Year;
                    creator.BannerPlayers = 1;
                    creator.Out = outputPath;
                    
                    // Create base WAD (this would need to be loaded from resources)
                    var baseWad = LoadBaseWadForConsole(targetConsole);
                    if (baseWad == null)
                    {
                        throw new Exception($"No base WAD available for {targetConsole}");
                    }
                    
                    // Inject ROM and create WAD
                    await Task.Run(() =>
                    {
                        injector.WAD = baseWad;
                        injector.Inject();
                        creator.MakeWAD(injector.WAD, new TitleImage());
                    });
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke(this, $"WAD injection failed: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Generate a unique Title ID for the game
        /// </summary>
        private string GenerateTitleId(GameTitle gameInfo)
        {
            // Generate a unique 4-character title ID based on game name
            var hash = gameInfo.Name.GetHashCode();
            var titleId = Math.Abs(hash).ToString("X4").PadLeft(4, '0');
            return titleId.Substring(0, 4);
        }

        /// <summary>
        /// Load base WAD file for the specified console
        /// </summary>
        private WAD LoadBaseWadForConsole(Console console)
        {
            // This would load the appropriate base WAD from resources
            // For now, return null - in a real implementation, you'd have
            // base WAD files embedded as resources or loaded from a directory
            
            StatusUpdated?.Invoke(this, $"Loading base WAD for {console}...");
            
            // TODO: Implement base WAD loading
            // You would need base WAD files for each console type
            
            return null;
        }

        /// <summary>
        /// Handle transfer progress updates from network communication
        /// </summary>
        private void OnTransferProgressUpdated(object sender, TransferStatus status)
        {
            if (status.OperationId == currentOperationId)
            {
                ProgressUpdated?.Invoke(this, (int)status.ProgressPercentage);
            }
        }

        /// <summary>
        /// Disconnect from the current device
        /// </summary>
        public void Disconnect()
        {
            connectedDevice = null;
            currentOperationId = null;
            StatusUpdated?.Invoke(this, "Disconnected from device");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (networkComm != null)
                {
                    networkComm.TransferProgressUpdated -= OnTransferProgressUpdated;
                }
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Namespace for console-specific injectors
    /// </summary>
    namespace Injectors
    {
        // These would be implementations of the existing injector classes
        // but adapted to work with the network ROM injector
        
        public class NES : InjectorWiiVC
        {
            public void Inject()
            {
                // NES-specific injection logic
                // This would be based on the existing NES.cs implementation
            }
        }
        
        public class SNES : InjectorWiiVC
        {
            public void Inject()
            {
                // SNES-specific injection logic
                // This would be based on the existing SNES.cs implementation
            }
        }
        
        public class N64 : InjectorWiiVC
        {
            public void Inject()
            {
                // N64-specific injection logic
                // This would be based on the existing N64.cs implementation
            }
        }
        
        public class SEGA : InjectorWiiVC
        {
            public void Inject()
            {
                // SEGA-specific injection logic
                // This would be based on the existing SEGA.cs implementation
            }
        }
    }
}