using Microsoft.EntityFrameworkCore;
using MurderBot.Data.Models;

namespace MurderBot.Data.Context;

public class MurderContext : DbContext
{
    
    public DbSet<AlwaysRemoveParticipant> AlwaysRemoveParticipant => Set<AlwaysRemoveParticipant>();
    public DbSet<AutoReaddToken> AutoReaddToken => Set<AutoReaddToken>();
    public DbSet<ChatMessage> ChatMessage => Set<ChatMessage>();
    public DbSet<ExemptParticipant> ExemptParticipant => Set<ExemptParticipant>();
    public DbSet<Group> Group => Set<Group>();
    public DbSet<GroupAutoReply> GroupAutoReply => Set<GroupAutoReply>();
    public DbSet<GroupCheckIn> GroupCheckIn => Set<GroupCheckIn>();
    public DbSet<GroupCheckInParticipantCheckIn> GroupCheckInParticipantCheckIn => Set<GroupCheckInParticipantCheckIn>();
    public DbSet<Participant> Participant => Set<Participant>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(entity =>
        {
            entity.Property(e => e.CheckInReadTimeout)
                .HasConversion<string>();
            
            entity.Property(e => e.LastMessageExemptTime)
                .HasConversion<string>();
            
            entity.Property(e => e.CheckInMessageResponseTimeout)
                .HasConversion<string>();
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
        });
    }
}

