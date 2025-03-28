using Newtonsoft.Json;

namespace MurderBot.Infrastructure.WassengerClient.Models.WebHooks;



public class ContactData
{
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }

    [JsonProperty("metadata")]
    public List<object> Metadata { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }
    

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("wid")]
    public string Wid { get; set; }
}
public class ChatData
{
    [JsonProperty("contact")]
    public ContactData Contact { get; set; }

    [JsonProperty("date")]
    public DateTimeOffset? Date { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }


}
public class MessageData
{
    [JsonProperty("ack")]
    public string Ack { get; set; }

    [JsonProperty("body")]
    public string Body { get; set; }
    
    [JsonProperty("date")]
    public DateTimeOffset? Date { get; set; }
    

    [JsonProperty("flow")]
    public string? Flow { get; set; }

    [JsonProperty("from")]
    public string? From { get; set; }

    [JsonProperty("fromNumber")]
    public string? FromNumber { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
    

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("timestamp")]
    public int? Timestamp { get; set; }

    [JsonProperty("to")]
    public string To { get; set; }

    [JsonProperty("toNumber")]
    public string ToNumber { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("chat")]
    public ChatData Chat { get; set; }
}

public class MessageWebhook
{
    [JsonProperty("created")]
    public int? Created { get; set; }

    [JsonProperty("data")]
    public MessageData Data { get; set; }

    [JsonProperty("event")]
    public string? Event { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("object")]
    public string Object { get; set; }
}