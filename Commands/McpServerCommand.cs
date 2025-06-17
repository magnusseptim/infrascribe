using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Spectre.Console.Cli;
using InfraScribe.CLI.Helpers;
using InfraScribe.CLI.Types;

namespace InfraScribe.CLI.Commands;

public sealed class McpServerCommand : AsyncCommand<McpServerCommand.Settings>
{

    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--port <PORT>")]
        public int Port { get; init; } = 5110;
    }

    public override async Task<int> ExecuteAsync(CommandContext _, Settings s)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.Configure<JsonOptions>(o =>
            o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

        var app = builder.Build();

        // /tools endpoint
        app.MapGet("/mcp/v1/tools", (HttpContext ctx) =>
        {
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync(JsonSerializer.Serialize(ToolMetadata.All));
        });

        // /run endpoint
        app.MapPost("/mcp/v1/run", async (HttpContext http) =>
        {
            string body = string.Empty;
            RunRequest? request = null;

            try
            {
                using var reader = new StreamReader(http.Request.Body);
                body = await reader.ReadToEndAsync();

                request = JsonSerializer.Deserialize<RunRequest>(body, _serializerOptions);

                if (request == null || !ToolMetadata.Index.TryGetValue(request.Tool, out var handler))
                {
                    http.Response.StatusCode = 400;
                    await http.Response.WriteAsync("Invalid tool or payload\n");
                    return;
                }

                http.Response.ContentType = "text/event-stream";

                await handler(request.Args, http.Response.BodyWriter);
            }
            catch (Exception ex)
            {
                if (!http.Response.HasStarted)
                    http.Response.ContentType = "text/plain";

                await http.Response.WriteAsync($"\n[ERROR IN ENDPOINT]: {ex.GetType().Name}: {ex.Message}\n");
            }
        });

        app.Urls.Add($"http://localhost:{s.Port}");
        await app.RunAsync();
        return 0;
    }
}
