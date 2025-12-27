using System.IO.Compression;

namespace Installer.Services;

public class MelonLoaderService
{
    private const string MelonLoaderFolder = "MelonLoader";
    private const string VersionDll = "version.dll";
    private const string MelonLoaderDownloadUrl =
        "https://github.com/LavaGang/MelonLoader/releases/download/v0.7.1/MelonLoader.x86.zip";

    /// <summary>
    /// Checks if MelonLoader is installed in the game directory.
    /// </summary>
    public bool IsMelonLoaderInstalled(string gamePath)
    {
        // Check for version.dll (MelonLoader proxy DLL)
        var versionDllPath = Path.Combine(gamePath, VersionDll);
        if (!File.Exists(versionDllPath))
        {
            return false;
        }

        // Check for MelonLoader folder
        var melonLoaderDir = Path.Combine(gamePath, MelonLoaderFolder);
        return Directory.Exists(melonLoaderDir);
    }

    /// <summary>
    /// Installs MelonLoader by downloading and extracting the zip archive.
    /// </summary>
    public async Task InstallMelonLoaderAsync(
        string gamePath,
        Action<string>? statusCallback = null
    )
    {
        var tempZipPath = Path.Combine(Path.GetTempPath(), "MelonLoader.x86.zip");

        try
        {
            // Download MelonLoader zip
            statusCallback?.Invoke("Downloading MelonLoader v0.7.1...");
            await DownloadFileAsync(MelonLoaderDownloadUrl, tempZipPath);

            // Extract to game directory
            statusCallback?.Invoke("Extracting MelonLoader files...");
            ExtractMelonLoader(tempZipPath, gamePath);

            // Verify installation
            if (!IsMelonLoaderInstalled(gamePath))
            {
                throw new Exception(
                    "MelonLoader extraction completed but verification failed. "
                        + "Please try installing MelonLoader manually from: "
                        + "https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.1"
                );
            }

            statusCallback?.Invoke("MelonLoader installed successfully.");
        }
        finally
        {
            // Clean up temp file
            try
            {
                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private async Task DownloadFileAsync(string url, string destinationPath)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PWAATAccessibilityInstaller/1.0");

        using var response = await httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead
        );
        response.EnsureSuccessStatusCode();

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            8192,
            true
        );
        await contentStream.CopyToAsync(fileStream);
    }

    private void ExtractMelonLoader(string zipPath, string gamePath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        foreach (var entry in archive.Entries)
        {
            // Skip directory entries
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            var destinationPath = Path.Combine(gamePath, entry.FullName);
            var destinationDir = Path.GetDirectoryName(destinationPath);

            // Create directory if needed
            if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // Extract file, overwriting if exists
            entry.ExtractToFile(destinationPath, overwrite: true);
        }
    }
}
