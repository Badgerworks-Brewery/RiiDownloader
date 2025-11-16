# HELP WANTED!
This needs some help to keep going, feel free to contribute if you want to!

# RiiDownloader
This is a patch for the Wii Shop Channel that transfers Wii games downloaded on the Switch to your Wii.

# How does this work?
Say you want to get (for example) mario 64 on the wii but you didnt originally buy the game so you cant redownload.
Now is where my idea comes in.
If you buy the game on switch (for example, Super Mario 3D All Stars) you go onto your wii and open this "patched" Wii Shop Channel where you would be able to link your Nintendo Account. The shop then looks at what games you own (like Playnite) and sees if any of the games you own were also on the Wii: then it takes you to the redownload game page on the Wii Shop Channel, it then finds the Switch on your network, uses nxdump remotely that extracts the game file, uses automated versions of modding tools to rip the game rom from it, then either applies any free patches or connects to the Open Shop Channel and installs one of its emulator and installs a forwarder on the HOME Menu. I know this is complicated, but it's worth a try!

## Recent Updates

### Network Integration (Issues #6 & #7)

The NXDump and WAD Packer components now communicate over the network! 🎉

**What's New:**
- Automatic device discovery for NXDump on local network
- Browse games available on your Nintendo Switch
- Extract games over network
- Automatically create WAD files for Wii Virtual Console
- Comprehensive documentation and error handling

**How to Use:**
1. Open FriishProduce (WAD Repackager)
2. Click "NXDump Network" button in the ribbon
3. Connect to your Switch running NXDump
4. Select a game and target console
5. Extract and create WAD file

**Documentation:**
- [Network Integration Guide](WAD%20Repackager/NETWORK_INTEGRATION.md) - Technical details and protocol
- [Implementation Summary](WAD%20Repackager/IMPLEMENTATION_SUMMARY.md) - Status and roadmap

**Architecture:**
```
Nintendo Switch (NXDump) ←→ Network ←→ FriishProduce (WAD Packer)
     [Game Data]                             [WAD Files]
```

The integration handles:
- Device discovery (UDP broadcast)
- Game extraction requests (HTTP API)
- File transfer with progress tracking
- ROM injection into WAD format
- Support for NES, SNES, N64, and Genesis

See the documentation files for complete details on setup and usage.

