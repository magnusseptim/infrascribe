using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.IO.Pipelines;
using InfraScribe.CLI.Mcp;
using InfraScribe.CLI.Types;

namespace InfraScribe.CLI.Helpers;

public static partial class ToolMetadata
{
    // -------------------------------------------------
    // JSON Schemas
    // -------------------------------------------------
    private static readonly JsonNode DocSchema = JsonNode.Parse(@"{""type"":""object"",""properties"":{""templatePath"":{""type"":""string""},""outputDirectory"":{""type"":""string"",""default"":""./docs""},""llmSummary"":{""type"":""boolean"",""default"":false}},""required"":[""templatePath""]}")!;

    private static readonly JsonNode AskSchema = JsonNode.Parse(@"{""type"":""object"",""properties"":{""templatePath"":{""type"":""string""},""question"":{""type"":""string""}},""required"":[""templatePath"",""question""]}")!;

    public static readonly object[] All =
    {
        new { name = "doc", description = "Generate markdown documentation for a CloudFormation/CDK template", parameter_schema = DocSchema },
        new { name = "ask", description = "Ask questions about template and stream the LLM answer", parameter_schema = AskSchema }
    };

    // -------------------------------------------------
    // Shared JSON options (case‑insensitive)
    // -------------------------------------------------
    private static readonly JsonSerializerOptions _ciOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // -------------------------------------------------
    // Tool name → executor map
    // -------------------------------------------------
    public static readonly IReadOnlyDictionary<string, Func<JsonElement, PipeWriter, Task>> Index =
        new Dictionary<string, Func<JsonElement, PipeWriter, Task>>(StringComparer.OrdinalIgnoreCase)
        {
            ["doc"] = async (json, pipe) =>
            {
                var args = json.Deserialize<DocArgs>(_ciOpts)!;
                await Executor.RunDocAsync(args, pipe);
            },
            ["ask"] = async (json, pipe) =>
            {
                var args = json.Deserialize<AskArgs>(_ciOpts)!;
                await Executor.RunAskAsync(args, pipe);
            }
        };
    
}
