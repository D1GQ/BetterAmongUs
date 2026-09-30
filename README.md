# ♻ BetterAmongUs ♻

A client-sided mod that enhances the experience for the popular game Among Us!

<img width="700" height="500" alt="BetterAmongUs-Logo" src="/assets/BetterAmongUs-Logo.png" />

</p>
<p align="center">

<div style="text-align: center;">
    <a href="https://discord.gg/vjYrXpzNAn" target="_blank">
        <img src="https://img.shields.io/badge/Discord%20-%231DA1F2.svg?&style=for-the-badge&logo=discord&logoColor=white&color=5662f6" width="200" height="50"/>
    </a>
</div>

## Installation
### For First-Time Installation:

1. **Download the Correct Version**: 
   - Go to the [Releases](https://github.com/D1GQ/BetterAmongUs/releases) page
   - **Choose the correct zip file** for your platform:
     - Steam, Epic Games and Microsoft Store users: `BAU-Steam-Epic-MsStore-Ver.zip`
     - itch.io users: `BAU-Itchio-Ver.zip `

2. **Extract the Files**:
   - Extract the downloaded zip file to a temporary location

3. **Install to Among Us Folder**:
   - Navigate to your Among Us installation directory
   - Copy ALL files and folders from the extracted zip into your Among Us folder
   - Overwrite any existing files when prompted

4. **Verify Installation**:
   - Launch Among Us
   - If installed correctly, you should see "BetterAmongUs" in the main menu

---
### For Updating an Existing Installation:

1. **Download the DLL File**:
   - Go to the [Releases](https://github.com/D1GQ/BetterAmongUs/releases) page
   - Download just the `BetterAmongUs.dll` file

2. **Replace the Old DLL**:
   - Navigate to your Among Us installation folder
   - Go to: `BepInEx/plugins/`
   - Replace the existing `BetterAmongUs.dll` with the newly downloaded one

3. **Verify Update**:
   - Launch Among Us
   - Check that the mod version has been updated in the main menu

---
### Linux Setup (Steam Only)

Use Proton (9.0+) with these launch options:
```
WINEDLLOVERRIDES="winhttp=n,b" PROTON_NO_ESYNC=1 %command%
```

**Setup Steps:**
1. Enable Proton in Steam → Properties → Compatibility → Force Proton 9.0+
2. Copy all mod files into your Among Us directory
3. Add the launch options above
4. Install Protontricks and set `winhttp` as a library override in winecfg

---
### Android Setup

1. **Download Starlight**:
   - Go to the Google Play Store
   - Search for an app called **Starlight**
   - Download and install it

2. **Install BAU Through Starlight**:
   - Open Starlight and install BetterAmongUs through it

3. **Run the Mod Profile**:
   - Launch the mod profile to start playing with BetterAmongUs

## Supported Platforms
- ✅ Steam
- ✅ Linux + Steam
- ✅ Epic Games
- ✅ Microsoft Store
- ✅ itch.io
- ✅ Android
- ❌ iOS
- ❌ Xbox/Playstation/Switch

## Supported Game Versions
- ✅ AU **v19.0.0** / **v2026.9.29**: (BAU v1.3.4) >
- ✅ AU **v18.0.0** / **v2026.8.18**: (BAU v1.3.3 Hotfix 1) >
- ✅ AU **v17.2.0** / **v2026.3.17**: (BAU v1.3.3) >
- ✅ AU **v17.1.0** / **v2025.11.18**: (BAU v1.3.1) >
- ✅ AU **v17.0.1** / **v2025.10.14**: (BAU v1.3.0) >
- ✅ AU **v16.1.0** / **v2025.6.10**: (BAU v1.2.0 Beta 1) >
- ✅ AU **v16.0.0** / **v2025.3.25**: (BAU v1.1.6 Beta 1) >
- ✅ AU **v2024.11.26**: >
- ✅ AU **v2024.10.29**: >
- ✅ AU **v2024.9.4**: >
- ✅ AU **v2024.8.13**: >
- ✅ AU **v2024.6.18**: (BAU v1.0.0) >
- ❌ AU **v2024.3.5**: or Below <

## Features

BetterAmongUs adds a variety of improvements and tools to Among Us:

* **Anti-Cheat** - Detect and block invalid actions, RPCs, and known cheats.
* **Host Tools** - Additional options and controls for hosts.
* **Better Options** - More ways to customize your game.
* **Commands** - Commands for managing and interacting with the game.
* **Client Improvements** - Various client-side improvements and quality-of-life changes.
* **And More** - Additional features and improvements.

<img width="700" height="500" alt="freeplay-promo" src="/assets/freeplay-promo.png" />

## Anti-Cheat

BetterAmongUs includes a client-side anti-cheat that works for both hosts and non-hosts.

### Features

* Detect invalid actions and RPCs.
* Cancel invalid actions and RPCs.
* Detect known cheat clients.
* Store information about detected players.
* Additional checks to help prevent cheating.

## Commands

* `/help` - Show command help.
* `/commands` - List all available commands.
* `/dump` - Save the log to the desktop.
* `/player {id}` - Show information about a player.
* `/players` - Show information about all players.
* `/setprefix {prefix}` - Change the command prefix.
* `/kick {id}` - Kick a player. **Host Only**
* `/ban {id}` - Ban a player. **Host Only**
* `/forceskip {count votes}` - Force ends a meeting. **Host Only**
* `/endgame` - Force end the game. **Host Only**
* `/removeplayer {identifier}` - Remove a player from local Anti-Cheat data by Friend Code or HashPuid.
* `/removeall` - Remove all players from local Anti-Cheat data.

## Credits

A huge thank you to everyone who contributed to making BetterAmongUs a reality!

- **Head Developer**: [D1GQ](https://github.com/D1GQ)
- **Contributor**: [Nyx](https://github.com/DeveloperNyx)
- **Contributor**: [At0mBomba](https://github.com/At0mBomba)

## Contacts
betterauofficial@gmail.com

## Disclaimer

**BetterAmongUs** is an unofficial, fan-made mod for **Among Us**. It is not affiliated with, endorsed by, or associated with **InnerSloth LLC** or the official **Among Us** game. All trademarks and copyrights related to **Among Us** are the property of **InnerSloth LLC**. This mod is created purely for entertainment purposes and to enhance the gaming experience!
