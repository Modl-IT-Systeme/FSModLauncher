# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FSModLauncher is a WPF desktop application built with .NET 8 targeting Windows. It is a launcher/mod manager for Farming Simulator 25 that compares local mods with server-hosted lists, downloads missing/outdated mods, and launches the game.

## Architecture

- **Technology Stack**: WPF application using .NET 8 with C# latest language version
- **Pattern**: MVVM (Model-View-ViewModel) architecture using CommunityToolkit.Mvvm
- **UI Framework**: Windows Presentation Foundation (WPF) with XAML markup
- **Configuration**: Standard .NET SDK-style project with nullable reference types enabled
- **Logging**: Serilog for file-based logging
- **JSON**: Newtonsoft.Json for configuration serialization

## Project Structure

### Core Folders
- `Models/`: Data models (ServerMod, LocalMod, AppSettings, etc.)
- `Services/`: Business logic services (ConfigService, DownloadService, etc.)
- `ViewModels/`: MVVM ViewModels with commands and data binding
- `Views/`: Additional windows (SettingsWindow)
- `Converters/`: WPF value converters for data binding

### Key Files
- `FSModLauncher.sln`: Visual Studio solution file
- `FSModLauncher/FSModLauncher.csproj`: Main project file with .NET 8 and WPF configuration
- `FSModLauncher/App.xaml` & `App.xaml.cs`: WPF application entry point with global error handling
- `FSModLauncher/MainWindow.xaml` & `MainWindow.xaml.cs`: Main application window with mod list

## Dependencies

- **CommunityToolkit.Mvvm**: MVVM framework for commands and property notifications
- **Newtonsoft.Json**: JSON serialization for configuration
- **Serilog**: Structured logging framework
- **Serilog.Sinks.File**: File output for logs

## Features Implemented

1. **Configuration Management**: Settings stored in `%APPDATA%\Fs25ModLauncher\settings.json`
2. **Server Integration**: Fetches mod list from XML endpoint
3. **Local Mod Scanning**: Scans zip files in mods folder, extracts version from modDesc.xml
4. **Hash Verification**: Supports MD5, SHA1, or no verification
5. **Parallel Downloads**: Configurable concurrent download limit
6. **Backup System**: Optional backup before overwriting mods
7. **Game Launcher**: Launches FS25 executable
8. **Progress Tracking**: Individual and overall download progress
9. **Error Handling**: Comprehensive error handling with user feedback

## Development Commands

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project FSModLauncher
```

### Build for Release
```bash
dotnet build --configuration Release
```

### Restore Dependencies
```bash
dotnet restore
```

## Configuration

The application creates configuration at `%APPDATA%\Fs25ModLauncher\settings.json` with:
- **ModsFolder**: Path to FS25 mods directory
- **GameExePath**: Path to FarmingSimulator25.exe
- **ServerIp**: IP address of the FS25 dedicated server
- **ServerPort**: Port number of the web interface (typically 8080)
- **ServerCode**: API authentication code for server access
- **HashAlgorithm**: Verification method (MD5, SHA1, None)
- **ConcurrentDownloads**: Max parallel downloads (1-10)
- **BackupBeforeOverwrite**: Backup existing mods before update

The ServerIp, ServerPort, and ServerCode are combined to create:
- Server Stats URL: `http://{ServerIp}:{ServerPort}/feed/dedicated-server-stats.xml?code={ServerCode}`
- CDN Base URL: `http://{ServerIp}:{ServerPort}/mods`

## Project Configuration

- **Target Framework**: .NET 8 Windows-specific (`net8.0-windows`)
- **Output Type**: Windows executable (`WinExe`)
- **Language Features**: Latest C# with nullable reference types and implicit usings enabled
- **UI Framework**: WPF enabled (`UseWPF=true`)