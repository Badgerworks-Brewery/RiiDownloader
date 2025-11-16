# NXDump Network Integration

This document describes the network integration between NXDump (Nintendo Switch game extraction) and FriishProduce (Wii WAD creation).

## Overview

The network integration allows FriishProduce to automatically:
1. Discover NXDump devices on the local network
2. Connect to a Nintendo Switch running NXDump
3. Browse available game titles on the Switch
4. Request extraction of game data
5. Download extracted files
6. Process NSP files to extract ROM data
7. Create Wii WAD files for virtual console injection

## Architecture

### Components

#### 1. NetworkCommunication.cs
Handles low-level network communication with NXDump devices:
- **Device Discovery**: UDP broadcast-based discovery (port 8765)
- **HTTP API Client**: REST API communication for commands
- **File Transfer**: Streaming download with progress reporting

#### 2. NetworkRomInjector.cs
Orchestrates the end-to-end workflow:
- Manages connection to devices
- Coordinates extraction requests
- Processes NSP files to extract ROMs
- Integrates with existing WAD injectors

#### 3. NetworkIntegrationForm.cs
User interface for network operations:
- Device list view with auto-discovery
- Title browser for available games
- Console target selection
- Progress monitoring

## Network Protocol

### Discovery Protocol

**Request** (UDP Broadcast on port 8765):
```
FRIISH_DISCOVER
```

**Response** (UDP):
```
NXDUMP_RESPONSE|<port>|<device_name>|<version>
```

Example:
```
NXDUMP_RESPONSE|8080|My Nintendo Switch|1.0.0
```

### HTTP API Endpoints

Base URL: `https://<device_ip>:<port>`

#### Get Device Info
```
GET /api/info
Response: { "name": "string", "version": "string" }
```

#### List Available Titles
```
GET /api/titles
Response: [
  {
    "titleId": "string",
    "name": "string",
    "version": "string",
    "size": number,
    "type": "Cartridge|Digital",
    "isExtractable": boolean
  }
]
```

#### Request Extraction
```
POST /api/extract
Body: {
  "titleId": "string",
  "format": "nsp|xci",
  "includeUpdates": boolean,
  "includeDLC": boolean
}
Response: { "operationId": "string" }
```

#### Get Extraction Status
```
GET /api/status/{operationId}
Response: {
  "operationId": "string",
  "status": "Pending|InProgress|Completed|Failed",
  "bytesTransferred": number,
  "totalBytes": number,
  "errorMessage": "string"
}
```

#### Download Extracted File
```
GET /api/download/{operationId}
Response: Binary stream of extracted file
```

## ROM Extraction from NSP

NSP files are Nintendo Switch Package files that contain encrypted game data. The extraction process involves:

1. **NSP Structure**: PFS0 (Partition File System) container
2. **NCA Files**: Nintendo Content Archives (encrypted game data)
3. **RomFS**: Read-only file system within NCA
4. **ROM Location**: Known paths based on game type

### Known ROM Locations

Based on actual game dumps:

**Super Mario 3D All-Stars (Mario 64)**:
- Path: `/rom/Stardust_JP/01_UNSMJ3.002.bin`
- Note: This is the Shindou (Japanese) version
- NCA: Offset 1 (game content NCA)

**General Guidelines**:
- N64 ROMs: Usually in `/rom/` directory, formats: .z64, .n64, .v64, .bin
- NES ROMs: Usually in `/content/` or `/rom/`, formats: .nes, .bin
- SNES ROMs: Usually in `/content/` or `/rom/`, formats: .sfc, .smc, .bin
- Genesis ROMs: Usually in `/content/` or `/rom/`, formats: .md, .gen, .bin

### Extraction Tools

For full NSP extraction support, external tools are required:
- **hactool**: Nintendo Switch content tool for decrypting and extracting NCA files
- **keys.txt**: Switch encryption keys (prod.keys)

## Usage

### From the UI

1. Open FriishProduce
2. Click the "NXDump Network" button in the ribbon
3. Wait for devices to be discovered automatically, or click "Add Manually" to enter IP address
4. Select a device and click "Connect"
5. Browse the available titles
6. Select a title and choose the target console type
7. Click "Extract & Create WAD"
8. Choose output location for the WAD file
9. Wait for extraction and injection to complete

### Programmatic Usage

```csharp
using FriishProduce;

// Initialize network communication
var networkComm = new NetworkCommunication();

// Discover devices (automatic)
var devices = networkComm.GetDiscoveredDevices();

// Or add manually
var device = await networkComm.AddDeviceManually("192.168.1.100", 8080);

// Create injector
var injector = new NetworkRomInjector(networkComm);
await injector.ConnectToDevice(device);

// Get titles
var titles = await injector.GetAvailableTitles();

// Extract and inject
var success = await injector.ExtractAndInjectRom(
    selectedTitle,
    Console.N64,
    "output.wad"
);
```

## Security Considerations

1. **HTTPS**: The API uses HTTPS for secure communication
2. **Local Network Only**: Discovery is limited to local network broadcast domain
3. **No Authentication**: Current implementation assumes trusted local network
4. **Encryption Keys**: NSP decryption requires user-provided keys

## Future Enhancements

- [ ] Implement full NSP parsing without external tools
- [ ] Add support for XCI (game cartridge) format
- [ ] Automatic detection of ROM type from file headers
- [ ] Batch processing of multiple titles
- [ ] Cloud storage integration for extracted ROMs
- [ ] Automatic ROM patching (language, bug fixes)
- [ ] Integration with GameTDB for metadata

## Troubleshooting

### Device Not Discovered
- Ensure both devices are on the same network subnet
- Check firewall settings (UDP port 8765, TCP port 8080)
- Try manual device addition with IP address

### Extraction Fails
- Verify Switch has enough storage space
- Check that title is extractable (not corrupted)
- Ensure NXDump has proper permissions

### ROM Not Found in NSP
- Different games may have different ROM locations
- Some games may not contain extractable ROMs (ports vs emulated games)
- Manual ROM extraction with hactool may be required

## References

- [NXDump Tool](https://github.com/DarkMatterCore/nxdumptool)
- [FriishProduce](https://github.com/saulfabregwiivc/FriishProduce)
- [hactool](https://github.com/SciresM/hactool)
- [Nintendo Switch Brew](https://switchbrew.org/)
