using FSModLauncher.Models;

namespace FSModLauncher.Services;

public class ComparerService
{
    public List<ComparisonResult> CompareModsAsync(List<ServerMod> serverMods, List<LocalMod> localMods,
        string hashAlgorithm)
    {
        var results = new List<ComparisonResult>();

        foreach (var serverMod in serverMods)
        {
            var localMod = localMods.FirstOrDefault(l =>
                string.Equals(l.Name, serverMod.Name, StringComparison.OrdinalIgnoreCase));

            var status = DetermineModStatus(serverMod, localMod, hashAlgorithm);

            results.Add(new ComparisonResult
            {
                ServerMod = serverMod,
                LocalMod = localMod,
                Status = status
            });
        }

        return results;
    }

    private ModStatus DetermineModStatus(ServerMod serverMod, LocalMod? localMod, string hashAlgorithm)
    {
        if (localMod == null) return ModStatus.Missing;

        // If hash algorithm is not "None" and both mods have hashes, compare hashes
        if (hashAlgorithm != "None" &&
            !string.IsNullOrEmpty(serverMod.Hash) &&
            !string.IsNullOrEmpty(localMod.Hash))
            return string.Equals(serverMod.Hash, localMod.Hash, StringComparison.OrdinalIgnoreCase)
                ? ModStatus.Latest
                : ModStatus.UpdateAvailable;

        // Fallback to version comparison
        if (!string.IsNullOrEmpty(serverMod.Version) && !string.IsNullOrEmpty(localMod.Version))
            return CompareVersions(localMod.Version, serverMod.Version) >= 0
                ? ModStatus.Latest
                : ModStatus.UpdateAvailable;

        // If we can't determine, assume update is available
        return ModStatus.UpdateAvailable;
    }

    private int CompareVersions(string version1, string version2)
    {
        try
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1.CompareTo(v2);
        }
        catch
        {
            // If version parsing fails, do string comparison
            return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
        }
    }
}