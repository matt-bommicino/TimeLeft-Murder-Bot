using Microsoft.Extensions.Options;
using MuderBot.Infrastructure.Settings;
using MuderBot.Infrastructure.WassengerClient.Models;
using Newtonsoft.Json;

namespace MuderBot.Infrastructure.WassengerClient;

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
    
    
    
}