using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace PdfCollector.Infrastructure.Services;

[DataContract]
public class AppSettings
{
    [DataMember] public bool   DeleteAfterZip      { get; set; }
    [DataMember] public bool   SaveLog             { get; set; }
    [DataMember] public bool   CloseAfterDone      { get; set; }
    [DataMember] public string LastDirectory       { get; set; } = string.Empty;
    [DataMember] public bool   AutoCheckForUpdates      { get; set; } = true;
    [DataMember] public bool   SumatraPdfPromptDeclined { get; set; }
}

public class AppSettingsService
{
    private static readonly string SettingsPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfCollector.settings.json");

    private AppSettings _current;

    public AppSettings Settings => _current ??= Load();

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return new AppSettings();
            var bytes = File.ReadAllBytes(SettingsPath);
            using var ms  = new MemoryStream(bytes);
            var ser = new DataContractJsonSerializer(typeof(AppSettings));
            return (AppSettings)ser.ReadObject(ms) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            using var ms  = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(AppSettings));
            ser.WriteObject(ms, settings);
            File.WriteAllBytes(SettingsPath, ms.ToArray());
        }
        catch { }
    }
}
