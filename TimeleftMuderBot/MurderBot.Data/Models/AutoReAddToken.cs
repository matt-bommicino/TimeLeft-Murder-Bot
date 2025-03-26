using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

/// <summary>
/// This represents a token a user can use to readd themselves
/// </summary>
public class AutoReAddToken : IDateCreated
{
    private readonly ILazyLoader _lazyLoader;

    public AutoReAddToken()
    {
        
    }

    private AutoReAddToken(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
    private GroupCheckIn _groupCheckIn = null!;

    [Key]
    public Guid TokenGuid { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset? ExpirationDate { get; set; }
    
    public DateTimeOffset? DateClaimed { get; set; }
    
    public int GroupCheckinId { get; set; }
    
    [StringLength(30)]
    public required string ParticipantId { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }

    public GroupCheckIn GroupCheckIn
    {
        get => _lazyLoader.Load(this, ref _groupCheckIn!)!;
        set => _groupCheckIn = value;
    }
}