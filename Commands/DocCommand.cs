// DOC COMMAND

using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using InfraScribe.CLI.Resolvers;
using InfraScribe.CLI.Utils;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace InfraScribe.CLI.Commands;

public class DocCommand : Command<DocCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<file>")]
        [Description("Path to the CloudFormation/CDK template (JSON or YAML).")]
        public string TemplatePath { get; set; }

        [CommandOption("--output <path>")]
        [Description("Custom output file path (overrides config).")]
        public string OutputPath { get; set; }

        [CommandOption("--no-llm")]
        [Description("Disable LLM summary generation for this run.")]
        public bool NoLLM { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var config = Config.Load();
        if (!string.IsNullOrWhiteSpace(settings.OutputPath))
            config.OutputDirectory = Path.GetDirectoryName(settings.OutputPath);
        if (settings.NoLLM)
            config.EnableLLMSummary = false;
        Console.WriteLine($"Generating docs for {settings.TemplatePath}...");

        if (!File.Exists(settings.TemplatePath))
        {
            Console.WriteLine("Template file not found.");
            return 1;
        }

        try
        {
            JsonNode? doc = null;
            var raw = File.ReadAllText(settings.TemplatePath).TrimStart();

            if (raw.StartsWith("{") || raw.StartsWith("["))
            {
                doc = JsonNode.Parse(raw);
            }
            else
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .WithNodeTypeResolver(new AwsTagIgnoringResolver())
                    .IgnoreUnmatchedProperties()
                    .Build();

                var yamlObject = deserializer.Deserialize(new StringReader(raw));

                var serializer = new SerializerBuilder()
                    .JsonCompatible()
                    .Build();
                var json = serializer.Serialize(yamlObject);

                doc = JsonNode.Parse(json);
            }

            if (doc == null || !doc.AsObject().ContainsKey("Resources"))
            {
                Console.WriteLine("Invalid template or missing 'Resources' section.");
                return 1;
            }

            var groupedResources = doc["Resources"]!.AsObject()
                .GroupBy(r => r.Value?["Type"]?.ToString() ?? "Unknown")
                .OrderBy(g => g.Key);

            var sb = new StringBuilder();
            sb.AppendLine("# Infrastructure Documentation");
            sb.AppendLine("## Resources");

            StreamWriter? logWriter = null;
            string? logPath = null;
            if (config.EnableLLMSummary)
            {
                Directory.CreateDirectory(config.OutputDirectory);
                logPath = Path.Combine(config.OutputDirectory, config.LogFile);
                logWriter = new StreamWriter(logPath, append: true);
            }

            foreach (var group in groupedResources)
            {
                sb.AppendLine($"### {group.Key}");
                foreach (var resource in group)
                {
                    sb.AppendLine($"- **{resource.Key}**");
                }

                if (config.EnableLLMSummary)
                {
                    var resourceJson = string.Join("", group.Select(r => r.Value.ToJsonString()));
                    var summaryPrompt = $"Summarize precisely the purpose of the following AWS resources defined in a CloudFormation template." +
                                        $"Avoid general AWS information. Be concise and list what these specific resources do. {resourceJson}";
                    
                    var summary = QueryOllama(summaryPrompt);
                    sb.AppendLine($"**Summary:**{summary.Trim()} ");
                    logWriter?.WriteLine("====================");
                    logWriter?.WriteLine($"🕒 Timestamp: {DateTime.Now}");
                    logWriter?.WriteLine($"🔍 Prompt:{summaryPrompt} ");
                    logWriter?.WriteLine($"💬 Response:{summary.Trim()} ");
                    logWriter?.WriteLine("====================");
                    
                    logWriter?.Flush();
                    logWriter?.Close();
                }
            }

            var outputPath = string.IsNullOrWhiteSpace(settings.OutputPath)
                ? Path.Combine(config.OutputDirectory, "infrastructure-doc.md")
                : settings.OutputPath;
            File.WriteAllText(outputPath, sb.ToString());
            Console.WriteLine($"Documentation written to {outputPath}");
            if (config.EnableLLMSummary && logPath != null)
                Console.WriteLine($"LLM usage logged to {logPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse template: {ex.Message}");
            return 1;
        }

        return 0;
    }


    private string QueryOllama(string prompt)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ollama",
                Arguments = $"run mistral \"{prompt.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                var output = new StringBuilder();
                while (!process.StandardOutput.EndOfStream)
                {
                    output.AppendLine(process.StandardOutput.ReadLine());
                }
                process.WaitForExit();
                return output.ToString();
            }
        }
        catch (Exception ex)
        {
            return $"Error calling Ollama: {ex.Message}";
        }
    }
}