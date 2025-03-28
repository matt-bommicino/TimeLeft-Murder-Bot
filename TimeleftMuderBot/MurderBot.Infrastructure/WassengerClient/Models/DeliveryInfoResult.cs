using Newtonsoft.Json;

namespace MurderBot.Infrastructure.WassengerClient.Models;


public class DeliveryParticipant
{
    [JsonProperty("wid")]
    public string Wid { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }

    [JsonProperty("timestamp")]
    public int Timestamp { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }
}

public class DeliveryInfoResult
{
    [JsonProperty("delivery")]
    public List<DeliveryParticipant> Delivery { get; set; }

    [JsonProperty("deliveryRemaining")]
    public int DeliveryRemaining { get; set; }

    [JsonProperty("read")]
    public List<DeliveryParticipant> Read { get; set; }

    [JsonProperty("readRemaining")]
    public int ReadRemaining { get; set; }
    
}