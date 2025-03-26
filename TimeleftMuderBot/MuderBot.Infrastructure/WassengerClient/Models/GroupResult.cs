using Newtonsoft.Json;

namespace MuderBot.Infrastructure.WassengerClient.Models;

public class GroupResult
{
    [JsonProperty("device")]
    public string Device { get; set; } = null!;

    [JsonProperty("wid")]
    public string Wid { get; set; } = null!;

    [JsonProperty("communityParentGroup")]
    public string CommunityParentGroup { get; set; }

    [JsonProperty("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("isArchive")]
    public bool IsArchive { get; set; }

    [JsonProperty("isCommunityAnnounce")]
    public bool IsCommunityAnnounce { get; set; }

    [JsonProperty("isPinned")]
    public bool IsPinned { get; set; }

    [JsonProperty("isReadOnly")]
    public bool IsReadOnly { get; set; }

    [JsonProperty("kind")]
    public string Kind { get; set; } = null!;

    [JsonProperty("lastMessageAt")]
    public DateTimeOffset? LastMessageAt { get; set; }

    [JsonProperty("lastSyncAt")]
    public DateTimeOffset? LastSyncAt { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("participants")]
    public List<GroupParticipantResult>? Participants { get; set; }

    [JsonProperty("totalParticipants")]
    public int? TotalParticipants { get; set; }

    [JsonProperty("unreadCount")]
    public int? UnreadCount { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; } = null!;
}