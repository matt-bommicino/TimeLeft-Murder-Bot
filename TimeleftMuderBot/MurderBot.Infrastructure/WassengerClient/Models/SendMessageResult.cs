using Newtonsoft.Json;

namespace MurderBot.Infrastructure.WassengerClient.Models;

public class SendMessageResult
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("waId")]
    public string WaId { get; set; }

    [JsonProperty("group")]
    public string? Group { get; set; }

    [JsonProperty("wid")]
    public string Wid { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("deliveryStatus")]
    public string? DeliveryStatus { get; set; }

    [JsonProperty("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonProperty("deliverAt")]
    public DateTimeOffset? DeliverAt { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
    
}