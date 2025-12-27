using Installer.Utils;
using Microsoft.Win32;

namespace Installer.Services;

public class GamePathFinder
{
    private const string DefaultSteamPath = @"C:\Program Files (x86)\Steam";
    private const string GameFolderName = "Phoenix Wright Ace Attorney Trilogy";
    private const string GameExecutable = "PWAAT.exe";

    /// <summary>
    /// Attempts to find the game installation path automatically.
    /// </summary>
    public string? FindGamePath()
    {
        // 1. Check default Steam location
        var defaultGamePath = Path.Combine(DefaultSteamPath, "steamapps", "common", GameFolderName);
        if (ValidateGamePath(defaultGamePath))
        {
            return defaultGamePath;
        }

        // 2. Get Steam path from registry
        var steamPath = GetSteamPathFromRegistry();
        if (steamPath != null)
        {
            // Check the main Steam library
            var mainLibraryPath = Path.Combine(steamPath, "steamapps", "common", GameFolderName);
            if (ValidateGamePath(mainLibraryPath))
            {
                return mainLibraryPath;
            }

            // 3. Parse libraryfolders.vdf for additional libraries
            var libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (File.Exists(libraryFoldersPath))
            {
                var vdfContent = File.ReadAllText(libraryFoldersPath);
                var libraryPaths = VdfParser.ParseLibraryFolders(vdfContent);

                foreach (var libraryPath in libraryPaths)
                {
                    var gamePath = Path.Combine(libraryPath, "steamapps", "common", GameFolderName);
                    if (ValidateGamePath(gamePath))
                    {
                        return gamePath;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the Steam installation path from the Windows registry.
    /// </summary>
    private string? GetSteamPathFromRegistry()
    {
        try
        {
            // Try 64-bit registry view first
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            if (key != null)
            {
                var installPath = key.GetValue("InstallPath") as string;
                if (!string.IsNullOrEmpty(installPath) && Directory.Exists(installPath))
                {
                    return installPath;
                }
            }

            // Fallback to 32-bit registry view
            using var key32 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (key32 != null)
            {
                var installPath = key32.GetValue("InstallPath") as string;
                if (!string.IsNullOrEmpty(installPath) && Directory.Exists(installPath))
                {
                    return installPath;
                }
            }
        }
        catch
        {
            // Registry access may fail, ignore and continue
        }

        return null;
    }

    /// <summary>
    /// Validates that the given path is a valid game installation.
    /// </summary>
    public bool ValidateGamePath(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return false;
        }

        // Check for the game executable
        var exePath = Path.Combine(path, GameExecutable);
        return File.Exists(exePath);
    }
}
