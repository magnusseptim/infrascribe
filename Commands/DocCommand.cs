// DOC COMMAND

using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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
            sb.AppendLine();
            sb.AppendLine("## Resources");
            sb.AppendLine();

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
                // Header with resource type and count
                var count = group.Count();
                sb.AppendLine($"### {group.Key} ({count} resource{(count > 1 ? "s" : "")})");
                sb.AppendLine();

                // Bullet list, tight to the header
                foreach (var resource in group)
                {
                    sb.AppendLine($"- **{resource.Key}**");
                }
                sb.AppendLine();

                // Optional: LLM summary for each group
                if (config.EnableLLMSummary)
                {
                    var resourceJson = string.Join("", group.Select(r => r.Value.ToJsonString()));
                    var summaryPrompt =
                        $"Summarize the specific purpose and configuration of each AWS resource, avoiding boilerplate like 'This AWS resource is ...'." +
                        $"Just describe what is unique and relevant. {resourceJson} Keep it concise." +
                        $"If resources are similar, summarize what they have in common and only mention differences. " +
                        $"Flag any obvious security concerns. ";
                    
                    var summary = QueryOllama(summaryPrompt).Trim();
                    sb.AppendLine(summary);
                    sb.AppendLine();

                    logWriter?.WriteLine("====================");
                    logWriter?.WriteLine($"🕒 Timestamp: {DateTime.Now}");
                    logWriter?.WriteLine($"🔍 Prompt:{summaryPrompt} ");
                    logWriter?.WriteLine($"💬 Response:{summary} ");
                    logWriter?.WriteLine("====================");
                }
            }

            if (logWriter != null)
            {
                logWriter.Close();
                logWriter.Dispose();
            }

            var outputPath = string.IsNullOrWhiteSpace(settings.OutputPath)
                ? Path.Combine(config.OutputDirectory, "infrastructure-doc.md")
                : settings.OutputPath;
            
            
            var markdown = sb.ToString();
            markdown = Regex.Replace(markdown, @"(\r?\n){3,}", "\n\n");
            
            File.WriteAllText(outputPath, markdown);
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