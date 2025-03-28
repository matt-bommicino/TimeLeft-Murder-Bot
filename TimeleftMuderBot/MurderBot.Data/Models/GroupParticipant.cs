using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

[PrimaryKey(nameof(GroupId), nameof(ParticipantId))]
public class GroupParticipant : IDateCreated
{
    private readonly ILazyLoader _lazyLoader;
    private Group _group = null!;
    private Participant _participant = null!;

    public GroupParticipant()
    {
        
    }

    private GroupParticipant(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
   
    [StringLength(50)]
    public required string GroupId { get; set; }
    

    [StringLength(50)]
    public required string ParticipantId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset? LastGroupMessage { get; set; }
    
    public bool IsAdmin { get; set; }
    
    public bool IsOwner { get; set; }

    public Group Group
    {
        get => _lazyLoader.Load(this, ref _group!)!;
        set => _group = value;
    }

    public Participant Participant
    {
        get => _lazyLoader.Load(this, ref _participant!)!;
        set => _participant = value;
    }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}