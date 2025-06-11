using Spectre.Console.Cli;
using System.Reflection;

namespace InfraScribe.CLI.Commands;

public class VersionCommand : Command<VersionCommand.Settings>
{
    public class Settings : CommandSettings
    {
        // No options for version
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        Console.WriteLine($"InfraScribe CLI version {version}");
        return 0;
    }
}