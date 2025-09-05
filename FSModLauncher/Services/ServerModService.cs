using System.Net.Http;
using System.Xml.Linq;
using FSModLauncher.Models;

namespace FSModLauncher.Services;

public class ServerModService
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public async Task<List<ServerMod>> FetchServerModsAsync(string serverStatsUrl)
    {
        try
        {
            var xmlContent = await _httpClient.GetStringAsync(serverStatsUrl);
            return ParseServerMods(xmlContent).Where(mod => !mod.Name.StartsWith("pdlc_")).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to fetch server mods: {ex.Message}", ex);
        }
    }

    private static List<ServerMod> ParseServerMods(string xmlContent)
    {
        try
        {
            var document = XDocument.Parse(xmlContent);
            var mods = new List<ServerMod>();

            var modsElement = document.Descendants("Mods").First();
            var modElements = modsElement.Descendants("Mod");
            foreach (var mod in modElements)
            {
                var name = mod.Attribute("name")?.Value ?? "";
                var author = mod.Attribute("author")?.Value ?? "";
                var version = mod.Attribute("version")?.Value ?? "";
                var hash = mod.Attribute("hash")?.Value ?? "";
                var title = mod.Value;

                if (!string.IsNullOrEmpty(name))
                    mods.Add(new ServerMod
                    {
                        Name = name,
                        Author = author,
                        Version = version,
                        Hash = hash,
                        Title = title
                    });
            }

            return mods;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse server mods XML: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}