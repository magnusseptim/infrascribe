using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using Spectre.Console.Cli;
using InfraScribe.CLI.Resolvers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class AskCommand : Command<AskCommand.Settings>
{
    public static Action<string>? GlobalOnAnswer;

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<file>")]
        [Description("Path to the CloudFormation/CDK template (JSON or YAML).")]
        public string TemplatePath { get; set; }

        [CommandArgument(1, "<question>")]
        [Description("Your question about the infrastructure.")]
        public string Question { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        Console.WriteLine($"Looking for file at: {settings.TemplatePath}");
        Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");

        if (string.IsNullOrWhiteSpace(settings.TemplatePath) || string.IsNullOrWhiteSpace(settings.Question))
        {
            Console.WriteLine("Both template path and question are required.");
            return 1;
        }

        var raw = File.ReadAllText(settings.TemplatePath).TrimStart();
        JsonNode? doc;

        try
        {
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
                var serializer = new SerializerBuilder().JsonCompatible().Build();
                var json = serializer.Serialize(yamlObject);
                doc = JsonNode.Parse(json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse template: {ex.Message}");
            return 1;
        }

        var contextPrompt = $"Here is an AWS CloudFormation/CDK template in JSON format:{doc}Question: {settings.Question}";

        Console.Write("Thinking");
        for (int i = 0; i < 3; i++) { Thread.Sleep(500); Console.Write("."); }
        Console.WriteLine();

        var answer = QueryOllama(contextPrompt);

        Console.WriteLine("💬 LLM Answer:");
        foreach (var ch in answer.Trim())
        {
            Console.Write(ch);
            Thread.Sleep(8);
        }
        Console.WriteLine();

        GlobalOnAnswer?.Invoke(answer.Trim());

        return 0;
    }

    private string QueryOllama(string prompt)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ollama",
                Arguments = $"run mistral \"{prompt.Replace("\"", "\"")}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            var output = new StringBuilder();
            while (!process.StandardOutput.EndOfStream)
            {
                output.AppendLine(process.StandardOutput.ReadLine());
            }
            process.WaitForExit();
            return output.ToString();
        }
        catch (Exception ex)
        {
            return $"Error calling Ollama: {ex.Message}";
        }
    }
}