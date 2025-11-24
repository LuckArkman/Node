using System.Text.Json.Serialization;

namespace Dtos;

public class PongResponse
{
    [JsonPropertyName("$type")]
    public string Type { get; set; }

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}
