namespace FSModLauncher.Services;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class GiantsModHash
{
    /// <summary>
    /// Computes the GIANTS-style hash for a mod file:
    /// MD5( [raw file bytes] + [UTF-8 bytes of baseName] )
    /// where baseName defaults to the filename without extension.
    /// </summary>
    /// <summary>
    /// Computes the GIANTS-style hash for a mod file asynchronously:
    /// MD5( [raw file bytes] + [UTF-8 bytes of baseName] )
    /// </summary>
    public static async Task<string> ComputeAsync(string filePath, string? customBaseName = null)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var baseName = customBaseName ?? Path.GetFileNameWithoutExtension(filePath);

        using var md5 = MD5.Create();
        using var fs = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            true);

        var buffer = new byte[1024 * 1024]; // 1 MB buffer
        int read;
        while ((read = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0) md5.TransformBlock(buffer, 0, read, null, 0);

        // Append the baseName bytes
        var tail = Encoding.UTF8.GetBytes(baseName);
        md5.TransformFinalBlock(tail, 0, tail.Length);

        return BitConverter.ToString(md5.Hash!).Replace("-", "").ToLowerInvariant();
    }
}