using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class MurderJoke : IDateCreated, IDateModified
{
    public int MurderJokeId { get; set; }
    
    public int TimesTold { get; set; }
    
    public string JokeText { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    public DateTimeOffset DateModified { get; set; }
}