using System.Collections.Generic;

namespace PdfCollector.Core.Models;

public class UpdateInfo
{
    public string CurrentVersion { get; set; }
    public string NewVersion    { get; set; }
    public string ReleaseNotes  { get; set; }
    public int    GitHubAssetId { get; set; }
    public bool   IsUpdateAvailable { get; set; }
    public List<GithubReleaseInfo> IntermediateReleases { get; set; } = new();
}
