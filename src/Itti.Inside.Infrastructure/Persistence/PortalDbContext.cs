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

    public DbSet<CeoDeskPost> CeoDeskPosts => Set<CeoDeskPost>();

    public DbSet<Holiday> Holidays => Set<Holiday>();

    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();

    public DbSet<MedicalInsurancePolicy> MedicalInsurancePolicies => Set<MedicalInsurancePolicy>();

    public DbSet<PortalDocument> PortalDocuments => Set<PortalDocument>();

    public DbSet<PortalImage> PortalImages => Set<PortalImage>();

    public DbSet<RockstarAwardImage> RockstarAwardImages => Set<RockstarAwardImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<PortalImage>(image => image.Property(item => item.ContentType).HasMaxLength(ContentLimits.Label));

        // A picture that is removed leaves its message or award in place, without a picture.
        modelBuilder.Entity<CeoDeskPost>(post =>
        {
            post.Property(item => item.Title).HasMaxLength(ContentLimits.ShortText);

            // nvarchar(MAX): a CEO's message can be a long letter. The master form sets the practical limit.
            post.Property(item => item.Message);
            post.HasOne<PortalImage>().WithMany().HasForeignKey(item => item.ImageId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Holiday>(holiday =>
        {
            holiday.Property(item => item.Name).HasMaxLength(ContentLimits.ShortText);
            holiday.HasIndex(item => item.Date);
        });

        modelBuilder.Entity<TrainingSession>(session =>
        {
            session.Property(item => item.Title).HasMaxLength(ContentLimits.ShortText);
            session.Property(item => item.Category).HasMaxLength(ContentLimits.Label);
            session.Property(item => item.Time).HasMaxLength(ContentLimits.Label);
            session.Property(item => item.Mode).HasMaxLength(ContentLimits.Label);
            session.Property(item => item.Venue).HasMaxLength(ContentLimits.ShortText);
            session.Property(item => item.Trainer).HasMaxLength(ContentLimits.ShortText);
            session.Property(item => item.Audience).HasMaxLength(ContentLimits.ShortText);
            session.Property(item => item.Description).HasMaxLength(ContentLimits.LongText);
            session.Property(item => item.Registration).HasMaxLength(ContentLimits.ShortText);
        });

        modelBuilder.Entity<MedicalInsurancePolicy>(policy =>
        {
            policy.Property(item => item.Insurer).HasMaxLength(ContentLimits.ShortText);
            policy.Property(item => item.Administrator).HasMaxLength(ContentLimits.ShortText);
            policy.Property(item => item.PolicyNumber).HasMaxLength(ContentLimits.Label);
            policy.Property(item => item.SumInsured).HasMaxLength(ContentLimits.ShortText);
            policy.Property(item => item.Coverage).HasMaxLength(ContentLimits.ShortText);
            policy.Property(item => item.Benefits).HasMaxLength(ContentLimits.LongText);
            policy.Property(item => item.ClaimSteps).HasMaxLength(ContentLimits.LongText);
            policy.Property(item => item.NetworkHospitalsLink).HasMaxLength(ContentLimits.Address);
            policy.Property(item => item.HelpdeskName).HasMaxLength(ContentLimits.ShortText);
            policy.Property(item => item.HelpdeskPhone).HasMaxLength(ContentLimits.Label);
            policy.Property(item => item.HelpdeskEmail).HasMaxLength(ContentLimits.Email);
        });

        modelBuilder.Entity<PortalDocument>(document =>
        {
            document.HasKey(item => item.Key);
            document.Property(item => item.Key).HasMaxLength(ContentLimits.Label);
            document.Property(item => item.FileName).HasMaxLength(ContentLimits.ShortText);
            document.Property(item => item.ContentType).HasMaxLength(ContentLimits.ShortText);
        });

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
            award.Ignore(item => item.ImageIds);
        });

        // Deleting an award or one of its pictures removes the link between them.
        modelBuilder.Entity<RockstarAwardImage>(link =>
        {
            link.HasOne<RockstarAward>().WithMany().HasForeignKey(item => item.AwardId).OnDelete(DeleteBehavior.Cascade);
            link.HasOne<PortalImage>().WithMany().HasForeignKey(item => item.ImageId).OnDelete(DeleteBehavior.Cascade);
            link.HasIndex(item => new { item.AwardId, item.Position });
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
