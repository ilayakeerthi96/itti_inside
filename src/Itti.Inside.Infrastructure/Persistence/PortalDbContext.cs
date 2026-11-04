using Itti.Inside.Domain.Content;
using Microsoft.EntityFrameworkCore;

namespace Itti.Inside.Infrastructure.Persistence;

/// <summary>The SQL Server tables behind the landing page.</summary>
public sealed class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options)
        : base(options)
    {
        // The records are read-only snapshots. Saves hand them back whole, so nothing needs tracking.
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<HeroSlide> HeroSlides => Set<HeroSlide>();

    public DbSet<NewsItem> NewsItems => Set<NewsItem>();

    public DbSet<CustomerFeedbackEntry> CustomerFeedback => Set<CustomerFeedbackEntry>();

    public DbSet<BirthdayEntry> Birthdays => Set<BirthdayEntry>();

    public DbSet<RockstarAward> RockstarAwards => Set<RockstarAward>();

    public DbSet<WeddingAnnouncement> WeddingAnnouncements => Set<WeddingAnnouncement>();

    public DbSet<NewBornAnnouncement> NewBornAnnouncements => Set<NewBornAnnouncement>();

    public DbSet<PortalText> PortalTexts => Set<PortalText>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<HeroSlide>(slide =>
        {
            slide.Property(item => item.Title).HasMaxLength(ContentLimits.ShortText);
            slide.Property(item => item.Subtitle).HasMaxLength(ContentLimits.ShortText);
        });

        modelBuilder.Entity<NewsItem>().Property(item => item.Text).HasMaxLength(ContentLimits.LongText);

        modelBuilder.Entity<CustomerFeedbackEntry>(entry =>
        {
            entry.Property(item => item.Client).HasMaxLength(ContentLimits.ShortText);
            entry.Property(item => item.Team).HasMaxLength(ContentLimits.ShortText);
            entry.Property(item => item.Excerpt).HasMaxLength(ContentLimits.LongText);
        });

        modelBuilder.Entity<BirthdayEntry>(entry =>
        {
            entry.Property(item => item.EmployeeId).HasMaxLength(ContentLimits.EmployeeId);
            entry.Property(item => item.Name).HasMaxLength(ContentLimits.ShortText);
            entry.Property(item => item.Email).HasMaxLength(ContentLimits.Email);
            entry.Property(item => item.Location).HasMaxLength(ContentLimits.ShortText);
        });

        modelBuilder.Entity<RockstarAward>(award =>
        {
            award.Property(item => item.Title).HasMaxLength(ContentLimits.ShortText);
            award.Property(item => item.Excerpt).HasMaxLength(ContentLimits.LongText);
        });

        modelBuilder.Entity<WeddingAnnouncement>().Property(item => item.Text).HasMaxLength(ContentLimits.ShortText);

        modelBuilder.Entity<NewBornAnnouncement>().Property(item => item.Text).HasMaxLength(ContentLimits.ShortText);

        modelBuilder.Entity<PortalText>(text =>
        {
            text.HasKey(item => item.Key);
            text.Property(item => item.Key).HasConversion<string>().HasMaxLength(50);
            text.Property(item => item.Value).HasMaxLength(ContentLimits.LongText);
        });
    }
}
