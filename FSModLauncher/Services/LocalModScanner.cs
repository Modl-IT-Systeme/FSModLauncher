using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Xml.XPath;
using FSModLauncher.Models;

namespace FSModLauncher.Services;

public class LocalModScanner
{
    public async Task<List<LocalMod>> ScanLocalModsAsync(string modsFolder, string hashAlgorithm)
    {
        var localMods = new List<LocalMod>();

        if (!Directory.Exists(modsFolder)) return localMods;

        var zipFiles = Directory.GetFiles(modsFolder, "*.zip", SearchOption.TopDirectoryOnly);

        foreach (var zipFile in zipFiles)
            try
            {
                var localMod = await ProcessZipFileAsync(zipFile, hashAlgorithm);
                if (localMod != null) localMods.Add(localMod);
            }
            catch
            {
                // Skip problematic files
            }

        return localMods;
    }

    private async Task<LocalMod?> ProcessZipFileAsync(string zipFilePath, string hashAlgorithm)
    {
        var fileInfo = new FileInfo(zipFilePath);
        var modName = Path.GetFileNameWithoutExtension(zipFilePath);

        var localMod = new LocalMod
        {
            Name = modName,
            FilePath = zipFilePath,
            SizeBytes = fileInfo.Length,
            // Extract version from modDesc.xml
            Version = await ExtractVersionFromZipAsync(zipFilePath)
        };

        // Compute hash if required
        if (hashAlgorithm != "None") localMod.Hash = await GiantsModHash.ComputeAsync(zipFilePath, modName);

        return localMod;
    }

    private static async Task<string?> ExtractVersionFromZipAsync(string zipFilePath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            var modDescEntry = archive.Entries.FirstOrDefault(e =>
                e.Name.Equals("modDesc.xml", StringComparison.OrdinalIgnoreCase));

            if (modDescEntry == null)
                return null;

            await using var stream = modDescEntry.Open();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            var doc = XDocument.Parse(content);
            return doc.Root?.Element("version")?.Value.Trim();
        }
        catch
        {
            return null;
        }
    }
}