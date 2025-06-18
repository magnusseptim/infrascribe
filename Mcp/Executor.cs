using System.Buffers;
using System.Text;
using Spectre.Console;
using Spectre.Console.Cli;
using InfraScribe.CLI.Commands;
using System.IO.Pipelines;
using InfraScribe.CLI.Helpers;
using InfraScribe.CLI.Types;

namespace InfraScribe.CLI.Mcp;

internal static class Executor
{
    // --------------------------------------------------
    // DOC TOOL
    // --------------------------------------------------
    public static async Task RunDocAsync(DocArgs args, PipeWriter pipe)
    {
        IDictionary<string, string>? outputMarkdown = null;
        InfraScribe.CLI.Commands.DocCommand.GlobalOnMarkdown = md => outputMarkdown = md;

        bool success = true;
        string? error = null;

        await RunWithAppAsync(
            pipe,
            app => app.Configure(config => config.AddCommand<DocCommand>("doc")),
            new[] { "doc", args.TemplatePath, "--no-files" }
                .Concat(string.IsNullOrWhiteSpace(args.OutputPath)
                    ? Array.Empty<string>()
                    : new[] { "--output-path", args.OutputPath })
                .Concat(args.NoLLM ? new[] { "--no-llm" } : Array.Empty<string>())
                .ToArray(),
            async (exitCode) => {
                if (exitCode != 0)
                {
                    success = false;
                    error = $"Doc command failed (exit code {exitCode})";
                }
                var result = new DocResult(success, null, error, outputMarkdown);
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                pipe.Write(Encoding.UTF8.GetBytes("\n-----RESULT-----\n"));
                pipe.Write(Encoding.UTF8.GetBytes(json));
                await pipe.FlushAsync();
            }
        );

        InfraScribe.CLI.Commands.DocCommand.GlobalOnMarkdown = null;
    }



    // --------------------------------------------------
    // ASK TOOL
    // --------------------------------------------------
    public static async Task RunAskAsync(AskArgs args, PipeWriter pipe)
    {
        string? answer = null;
        bool success = true;
        string? error = null;

        AskCommand.GlobalOnAnswer = a => answer = a;

        await RunWithAppAsync(
            pipe,
            app => app.Configure(config => config.AddCommand<AskCommand>("ask")),
            new[] { "ask", args.TemplatePath, args.Question },
            async (exitCode) => {
                if (exitCode != 0)
                {
                    success = false;
                    error = $"Ask command failed (exit code {exitCode})";
                }
                var result = new AskResult(success, answer, error);
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                pipe.Write(Encoding.UTF8.GetBytes("\n-----RESULT-----\n"));
                pipe.Write(Encoding.UTF8.GetBytes(json));
                await pipe.FlushAsync();
            }
        );

        AskCommand.GlobalOnAnswer = null;
    }


    // --------------------------------------------------
    // SHARED EXECUTION
    // --------------------------------------------------
    private static async Task RunWithAppAsync(
        PipeWriter pipe,
        Action<CommandApp> configure,
        string[] args,
        Func<int, Task> writeResultAsync
    )
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new PipeAnsiConsoleOutput(pipe),
            Interactive = InteractionSupport.No,
            ColorSystem = ColorSystemSupport.TrueColor
        });

        var originalConsole = AnsiConsole.Console;
        AnsiConsole.Console = console;

        int exitCode = 0;
        try
        {
            var app = new CommandApp();
            configure(app);
            exitCode = await app.RunAsync(args);
            await pipe.FlushAsync();
            await writeResultAsync(exitCode); // <-- Write result after app run but before completing pipe!
        }
        catch (Exception ex)
        {
            // Write an error result before pipe closes
            await writeResultAsync(-1);
            var diagnostics = $"\n[InfraScribe MCP server error]: {ex.Message}\nStackTrace:\n{ex.StackTrace}";
            pipe.Write(Encoding.UTF8.GetBytes(diagnostics));
            await pipe.FlushAsync();
        }
        finally
        {
            await pipe.CompleteAsync();
            AnsiConsole.Console = originalConsole;
        }
    }

    // --------------------------------------------------
    // PIPE CONSOLE OUTPUT
    // --------------------------------------------------
    internal sealed class PipeAnsiConsoleOutput : IAnsiConsoleOutput
    {
        private readonly PipeWriter _pipe;
        private Encoding _encoding = Encoding.UTF8;

        public PipeAnsiConsoleOutput(PipeWriter pipe) => _pipe = pipe;

        public void Write(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var bytes = _encoding.GetBytes(text);
                _pipe.Write(bytes);
            }
        }

        public void WriteLine() => Write("\n");

        public int Width => Console.IsOutputRedirected ? 80 : Console.BufferWidth;
        public int Height => Console.IsOutputRedirected ? 24 : Console.BufferHeight;

        public Encoding Encoding => _encoding;
        public bool IsTerminal => false;
        public TextWriter Writer => TextWriter.Null;

        public void SetEncoding(Encoding encoding) => _encoding = encoding ?? Encoding.UTF8;
        public void SetForeground(ConsoleColor color) { }
        public void SetBackground(ConsoleColor color) { }
        public void ResetColor() { }
        public void Clear(bool home) { }
    }
}
