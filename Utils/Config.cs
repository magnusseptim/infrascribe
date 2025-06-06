using System.Text.Json;

namespace InfraScribe.CLI.Utils;

public class Config
{
    public bool EnableLLMSummary { get; set; } = true;
    public string OutputDirectory { get; set; } = Directory.GetCurrentDirectory();
    public string LogFile { get; set; } = "llm_usage.log";

    public static Config Load(string path = "infrascribe.config.json")
    {
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }
        return new Config();
    }

    public void Save(string path = "infrascribe.config.json")
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}