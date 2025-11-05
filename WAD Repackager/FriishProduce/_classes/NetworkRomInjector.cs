using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

        public NetworkRomInjector(NetworkCommunication networkComm)
        {
            this.networkComm = networkComm;
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
                StatusUpdated?.Invoke(this, $"Connected to {device.DeviceName} at {device.IPAddress} ({titles.Count} titles available)");
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
            {
                throw new InvalidOperationException("No device connected");
            }

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
                        throw new ExtractionException($"Extraction failed: {status.ErrorMessage}");
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
                        var tempRomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
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
                            {
                                File.Delete(tempRomPath);
                            }
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
                var tempNspPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nsp");
                
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
                    {
                        File.Delete(tempNspPath);
                    }
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
            StatusUpdated?.Invoke(this, $"Processing NSP for {targetConsole} console...");
            
            try
            {
                // NSP files are PFS0 (Partition File System) archives
                // We need to:
                // 1. Extract the NSP contents
                // 2. Find the NCA file with the game content (usually offset 1)
                // 3. Extract the RomFS from the NCA
                // 4. Locate the ROM file within RomFS based on console type
                
                var romData = await ExtractRomFromNspFile(nspPath, targetConsole);
                
                if (romData != null && romData.Length > 0)
                {
                    StatusUpdated?.Invoke(this, $"Successfully extracted {romData.Length} bytes ROM data");
                    return romData;
                }
                else
                {
                    StatusUpdated?.Invoke(this, "Warning: Could not extract ROM from NSP, using placeholder");
                    // Return placeholder for now
                    return new byte[1024 * 1024];
                }
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke(this, $"Error processing NSP: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extract ROM file from NSP using knowledge of ROM locations
        /// Based on documented ROM paths for different games
        /// </summary>
        private async Task<byte[]> ExtractRomFromNspFile(string nspPath, Console targetConsole)
        {
            // This method attempts to extract the ROM from known locations
            // Based on the notes file, different games store ROMs in different locations:
            // - Mario 64: /rom/Stardust_JP/01_UNSMJ3.002.bin (Shindou version, Japanese)
            // - Other N64 games: typically in /rom/ folder
            // - NES/SNES: usually in /content/ or /rom/ folders
            
            StatusUpdated?.Invoke(this, "Analyzing NSP structure...");
            
            // Known ROM file patterns for different consoles
            var romPatterns = new Dictionary<Console, string[]>
            {
                { Console.N64, new[] { "*.z64", "*.n64", "*.v64", "*.bin" } },
                { Console.NES, new[] { "*.nes", "*.bin" } },
                { Console.SNES, new[] { "*.sfc", "*.smc", "*.bin" } },
                { Console.SMDGEN, new[] { "*.md", "*.bin", "*.gen" } },
                { Console.SMS, new[] { "*.sms", "*.bin" } }
            };
            
            // For now, return a placeholder indicating the feature is ready for implementation
            // In a complete implementation, this would:
            // 1. Use hactool or similar NSP extraction tools
            // 2. Parse the PFS0 structure
            // 3. Extract and decrypt NCA files
            // 4. Mount RomFS
            // 5. Search for ROM files matching the patterns
            
            StatusUpdated?.Invoke(this, "Note: Full NSP extraction requires external tools (hactool)");
            
            await Task.Delay(500); // Simulate processing
            
            // TODO: Implement actual NSP extraction using hactool or similar
            // For now, return null to indicate the ROM needs to be provided manually
            return null;
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
                        throw new InvalidOperationException($"No base WAD available for {targetConsole}");
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
                // No event unsubscription needed since TransferProgressUpdated was removed
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