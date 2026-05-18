using System.Runtime.Serialization;

namespace PdfCollector.Core.Models;

[DataContract]
public class GithubReleaseInfo
{
    [DataMember(Name = "tag_name")]  public string       TagName    { get; set; }
    [DataMember(Name = "name")]      public string       Name       { get; set; }
    [DataMember(Name = "body")]      public string       Body       { get; set; }
    [DataMember(Name = "assets")]    public GithubAsset[] Assets    { get; set; }
    [DataMember(Name = "prerelease")]public bool         Prerelease { get; set; }
    [DataMember(Name = "created_at")]public string       CreatedAt  { get; set; }
}

[DataContract]
public class GithubAsset
{
    [DataMember(Name = "id")]                   public int    Id                  { get; set; }
    [DataMember(Name = "name")]                 public string Name                { get; set; }
    [DataMember(Name = "browser_download_url")] public string BrowserDownloadUrl  { get; set; }
    [DataMember(Name = "size")]                 public long   Size                { get; set; }
}
