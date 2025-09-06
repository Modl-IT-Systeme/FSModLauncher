# FSModLauncher

A WPF desktop application for managing Farming Simulator 25 mods. Compares local mods with server-hosted lists, downloads missing or outdated mods, and launches the game.

## Features

-   Compare local mods with server mod lists
-   Download missing and outdated mods automatically
-   Hash verification (MD5, SHA1, or none)
-   Parallel downloads with configurable limits
-   Backup system for existing mods
-   Game launcher integration
-   Progress tracking for downloads

## Requirements

-   Windows operating system
-   .NET 8 runtime
-   Farming Simulator 25

## Installation

1. Download the latest release
2. Extract to desired location
3. Run FSModLauncher.exe
4. Configure settings on first run

## Configuration

The application creates a configuration file at `%APPDATA%\FSModLauncher\settings.json` where you can set:

-   Mods folder path
-   Game executable path
-   Server connection details
-   Download preferences
-   Hash verification method

## Building from Source

```bash
dotnet build
```

To run the application:

```bash
dotnet run --project FSModLauncher
```
