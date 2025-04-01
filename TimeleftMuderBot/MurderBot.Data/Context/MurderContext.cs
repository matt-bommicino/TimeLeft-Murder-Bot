using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MurderBot.Data.Interface;
using MurderBot.Data.Models;

namespace MurderBot.Data.Context;

public class MurderContext : DbContext
{

    public MurderContext(DbContextOptions<MurderContext> options) : base(options)
    {
        
    }
    
    
    public DbSet<AlwaysRemoveParticipant> AlwaysRemoveParticipant => Set<AlwaysRemoveParticipant>();
    public DbSet<AutoReAddToken> AutoReAddToken => Set<AutoReAddToken>();
    public DbSet<ChatMessage> ChatMessage => Set<ChatMessage>();
    public DbSet<ExemptParticipant> ExemptParticipant => Set<ExemptParticipant>();
    public DbSet<Group> Group => Set<Group>();
    
    public DbSet<GroupParticipant> GroupParticipant => Set<GroupParticipant>();
    public DbSet<GroupAutoReply> GroupAutoReply => Set<GroupAutoReply>();
    
    public DbSet<GroupAutoReplyMessage> GroupAutoReplyMessage => Set<GroupAutoReplyMessage>();
    public DbSet<GroupCheckIn> GroupCheckIn => Set<GroupCheckIn>();
    
    public DbSet<GroupCheckInMessage> GroupCheckInMessage => Set<GroupCheckInMessage>();
    public DbSet<GroupCheckInParticipantCheckIn> GroupCheckInParticipantCheckIn => Set<GroupCheckInParticipantCheckIn>();
    public DbSet<Participant> Participant => Set<Participant>();
    
    public DbSet<MessageTemplate> MessageTemplate => Set<MessageTemplate>();

    public DbSet<MurderJoke> MurderJoke => Set<MurderJoke>();
    
    public DbSet<ReAddJobTrigger> ReAddJobTrigger => Set<ReAddJobTrigger>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Murder");
        
        
        
        modelBuilder.Entity<Group>(entity =>
        {
            entity.Property(e => e.CheckInReadTimeout)
                .HasConversion<string>();
            
            entity.Property(e => e.LastMessageExemptTime)
                .HasConversion<string>();
            
            entity.Property(e => e.CheckInMessageResponseTimeout)
                .HasConversion<string>();
            
            entity.Property(e => e.MinimumTimeBetweenRuns)
                .HasConversion<string>();
            
            entity.Property(i => i.MessageSendStageMaxRetries).HasDefaultValue(5);
            
            entity.Property(i => i.RemovalStageMaxRetries).HasDefaultValue(5);
            
            entity.Property(i => i.ReminderCheckinMessages).HasDefaultValue(0);
        });

        modelBuilder.Entity<GroupAutoReply>(e =>
        {
            e.HasOne(i => i.Group)
                .WithMany(i => i.AutoReplies).HasForeignKey(i => i.GroupId);
        });
        
        modelBuilder.Entity<GroupParticipant>(e =>
        {
            e.HasOne(i => i.Group)
                .WithMany(i => i.GroupParticipants).HasForeignKey(i => i.GroupId);

            e.HasOne(i => i.Participant)
                .WithMany().HasForeignKey(i => i.ParticipantId);
        });
        
        modelBuilder.Entity<GroupCheckIn>(e =>
        {
            e.HasOne(i => i.Group)
                .WithMany(i => i.GroupCheckIns).HasForeignKey(i => i.GroupId);

            e.Property(i => i.ChatMessageSendStageAttempts).HasDefaultValue(0);
            e.Property(i => i.RemovalStageAttempts).HasDefaultValue(0);
        });

        modelBuilder.Entity<MurderJoke>(e =>
        {
            e.Property(i => i.TimesTold).HasDefaultValue(0);
        });

        modelBuilder.Entity<ReAddJobTrigger>(e =>
        {
            e.HasOne(f => f.AutoReAddToken)
                .WithMany().HasForeignKey(t => t.TokenGuid);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var classType = entityType.ClrType;
            if (typeof(IDateCreated).IsAssignableFrom(classType))
            {
                modelBuilder.Entity(classType).Property<DateTimeOffset>("DateCreated")
                    .HasDefaultValueSql("sysdatetimeoffset()");
            }
            
            if (typeof(IDateModified).IsAssignableFrom(classType))
            {
                modelBuilder.Entity(classType).Property<DateTimeOffset>("DateModified")
                    .HasDefaultValueSql("sysdatetimeoffset()");
            }
        }
        
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var fk in entity.GetForeignKeys())
            {
                fk.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
        
    }
    
    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is IDateModified
                                 && (e.State == EntityState.Added || e.State == EntityState.Modified)))
        {
            var entity = (IDateModified)entry.Entity;
            entity.DateModified = DateTimeOffset.Now;
        }
        
        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is IDateCreated
                                 && (e.State == EntityState.Added)))
        {
            var entity = (IDateCreated)entry.Entity;
            if (entity.DateCreated == default)
                entity.DateCreated = DateTimeOffset.Now;
        }
        
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is IDateModified
                                 && (e.State == EntityState.Added || e.State == EntityState.Modified)))
        {
            var entity = (IDateModified)entry.Entity;
            entity.DateModified = DateTimeOffset.Now;
        }
        
        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is IDateCreated
                                 && (e.State == EntityState.Added)))
        {
            var entity = (IDateCreated)entry.Entity;
            if (entity.DateCreated == default)
                entity.DateCreated = DateTimeOffset.Now;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

