using System.Text.Json;
using System.Text.Json.Serialization;

namespace GYMPlanner.Infrastructure;

/// <summary>
/// Shared JSON options used both for persisting snapshots and for parsing the
/// AI output. camelCase + string enums keep stored JSON readable and let the
/// Claude response (which uses camelCase keys) deserialize without ceremony.
/// </summary>
internal static class AppJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
