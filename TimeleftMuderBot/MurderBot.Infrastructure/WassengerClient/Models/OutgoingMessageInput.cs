using Newtonsoft.Json;

namespace MurderBot.Infrastructure.WassengerClient.Models;

public class OutgoingMessageInput
{
    [JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
    public string? Phone { get; set; }
    
    [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
    public string? Group { get; set; }
    
    [JsonProperty("quote", NullValueHandling = NullValueHandling.Ignore)]
    public string? Quote { get; set; }
    
    [JsonProperty("message")]
    public required string Message { get; set; }
}