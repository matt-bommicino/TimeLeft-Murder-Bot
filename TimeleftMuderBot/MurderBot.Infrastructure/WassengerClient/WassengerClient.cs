using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.WassengerClient.Models;
using Newtonsoft.Json;

namespace MurderBot.Infrastructure.WassengerClient;

public class WassengerClient
{
    private readonly CommonMurderSettings _murderSettings;
    private readonly HttpClient _httpClient;
   

    public WassengerClient(IOptions<CommonMurderSettings> murderSettings, IHttpClientFactory clientFactory)
    {
        _murderSettings = murderSettings.Value;
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.wassenger.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("Token", _murderSettings.WassengerApiToken);
    }


    public async Task<List<GroupResult>> GetGroups()
    {
        var url = $"devices/{_murderSettings.WassengerDeviceId}/groups";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResult = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<GroupResult>>(jsonResult)!;
    }

    public async Task<GroupResult> GetGroupById(string id)
    {
        var url = $"devices/{_murderSettings.WassengerDeviceId}/groups/{id}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResult = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GroupResult>(jsonResult)!;
    }

    public async Task RemoveGroupParticipant(string phoneNumber, string groupId)
    {
        var url = $"devices/{_murderSettings.WassengerDeviceId}/groups/{groupId}/participants";
        
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(url, UriKind.Relative),
            Content = new StringContent($"[\"{phoneNumber}\"]")
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };
        
        var response = await _httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="groupId"></param>
    /// <returns>The response if there was an error. Empty string for success</returns>
    public async Task<string> AddGroupParticipant(string phoneNumber, string groupId)
    {
        var url = $"devices/{_murderSettings.WassengerDeviceId}/groups/{groupId}/participants";
        
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url, UriKind.Relative),
            Content = new StringContent($"{{\n  \"participants\": [\n {{\n \"phone\": \"{phoneNumber}\",\n \"admin\": false\n }}\n ]\n}}")
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };
        
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return $"{response.StatusCode}: {content}";
        }
        

        return String.Empty;
    }
    

    public async Task<SendMessageResult> SendMessage(OutgoingMessageInput input)
    {
        var url = "messages";
        
        var json = JsonConvert.SerializeObject(input);

        var content = new StringContent(json)
        {
            Headers =
            {
                ContentType = new MediaTypeHeaderValue("application/json")
            }
        };
        var response = await _httpClient.PostAsync(url, content);
        
        response.EnsureSuccessStatusCode();
        var jsonResult = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<SendMessageResult>(jsonResult)!;
    }

    public async Task<DeliveryInfoResult> GetDeliveryInfo(string messageId)
    {
        var url = $"chat/{_murderSettings.WassengerDeviceId}/messages/{messageId}/ackinfo";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResult = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<DeliveryInfoResult>(jsonResult)!;
    }
    
    
    
}