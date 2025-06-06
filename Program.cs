using InfraScribe.CLI.Commands;
using InfraScribe.CLI.Utils;
using Spectre.Console.Cli;

namespace InfraScribe.CLI;

public static class Program
{
    public static Config Config => Config.Load();

    public static int Main(string[] args)
    {
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
        });

        return app.Run(args);
    }
}