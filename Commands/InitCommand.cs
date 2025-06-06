using System.ComponentModel;
using InfraScribe.CLI.Utils;
using Spectre.Console.Cli;

namespace InfraScribe.CLI.Commands;

public class InitCommand : Command<InitCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--force")]
        [Description("Overwrite existing config file if it exists.")]
        public bool Force { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var path = "infrascribe.config.json";

        if (File.Exists(path) && !settings.Force)
        {
            Console.WriteLine("Config file already exists. Use --force to overwrite.");
            return 1;
        }

        var config = new Config();
        config.Save(path);
        Console.WriteLine($"Config file saved to {path}");
        return 0;
    }
}