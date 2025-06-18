using System.Reflection;
using InfraScribe.CLI.Commands;
using InfraScribe.CLI.Utils;
using Spectre.Console.Cli;

namespace InfraScribe.CLI;

public static class Program
{
    public static Config Config => Config.Load();

    public static int Main(string[] args)
    {
        if (args.Length == 1 && (args[0] == "--version" || args[0] == "-v"))
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
            Console.WriteLine($"InfraScribe CLI version {version}");
            return 0;
        }
        
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("infrascribe");

            config.AddCommand<InitCommand>("init")
                .WithDescription("Initialize a new InfraScribe project.");

            config.AddCommand<DocCommand>("doc")
                .WithDescription("Generate documentation from a CloudFormation or CDK template.");

            config.AddCommand<AskCommand>("ask")
                .WithDescription("Ask questions about your infrastructure template.");

            config.AddCommand<McpServerCommand>("mcp")
                .WithDescription("Run the MCP server for tool metadata and execution.");
            
            config.AddCommand<VersionCommand>("version")
                .WithDescription("Show CLI version information.");
        });

        return app.Run(args);
    }
}