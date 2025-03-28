using Newtonsoft.Json;

namespace MurderBot.Infrastructure.WassengerClient.Models;

public class GroupParticipantResult
{
    [JsonProperty("phone")]
    public string Phone { get; set; } = null!;

    [JsonProperty("wid")]
    public string Wid { get; set; } = null!;

    [JsonProperty("isAdmin")]
    public bool IsAdmin { get; set; }

    [JsonProperty("isOwner")]
    public bool IsOwner { get; set; }
}