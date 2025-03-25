﻿using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

/// <summary>
/// This represents a token a user can use to readd themselves
/// </summary>
public class AutoReaddToken : IDateCreated
{
    [Key]
    public Guid TokenGuid { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset? ExpirationDate { get; set; }
    
    public DateTimeOffset? DateClaimed { get; set; }
    
    public int GroupCheckinId { get; set; }
    
    public required string ParticipantId { get; set; }
}