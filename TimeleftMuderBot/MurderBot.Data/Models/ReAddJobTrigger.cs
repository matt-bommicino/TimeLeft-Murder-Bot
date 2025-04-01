using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class ReAddJobTrigger : IDateCreated, IDateModified
{
    private readonly ILazyLoader _lazyLoader;
    private AutoReAddToken _autoReAddToken;

    public ReAddJobTrigger()
    {
        
    }

    private ReAddJobTrigger(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
    public int ReAddJobTriggerId { get; set; }
    
    public Guid TokenGuid { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset DateModified { get; set; }
    
    public DateTimeOffset? JobStartDate { get; set; }
    
    public DateTimeOffset? JobCompleteDate { get; set; }
    
    [StringLength(50)]
    public string? StartMessageId { get; set; }
    
    [StringLength(50)]
    public string? SuccessMessageId { get; set; }
    
    [StringLength(50)]
    public string? FailureMessageId { get; set; }
    
    public int RetryCount { get; set; }

    public AutoReAddToken AutoReAddToken
    {
        get => _lazyLoader.Load(this, ref _autoReAddToken!)!;
        set => _autoReAddToken = value;
    }
}