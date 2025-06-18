// -------------------------------------------------
// DTOs matching parameter schemas
// -------------------------------------------------
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InfraScribe.CLI.Types;

internal sealed record DocArgs(
[property: JsonPropertyName("templatePath")] string TemplatePath,
[property: JsonPropertyName("outputDirectory")] string? OutputPath = null,
[property: JsonPropertyName("llmSummary")] bool NoLLM = false);

internal sealed record AskArgs
{
    [JsonPropertyName("templatePath")] public string TemplatePath { get; init; } = string.Empty;
    [JsonPropertyName("question")] public string Question { get; init; } = string.Empty;
}

// -------------------------------------------------
// RunRequest used by MCP server POST /run endpoint
// -------------------------------------------------
internal sealed record RunRequest
{
    [JsonPropertyName("tool")]
    public string Tool { get; init; } = string.Empty;

    [JsonPropertyName("args")]
    public JsonElement Args { get; init; }
}


// -------------------------------------------------
// MCP Server Results
// -------------------------------------------------
internal sealed record DocResult(
    bool Success,
    string[]? OutputFiles = null,
    string? Error = null,
    IDictionary<string, string>? outputMarkdown = null);
internal sealed record AskResult(
    bool Success,
    string? Answer = null,
    string? Error = null);
