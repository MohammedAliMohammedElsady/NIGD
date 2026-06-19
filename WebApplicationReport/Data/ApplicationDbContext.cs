using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplicationReport.Models.Domain;

namespace WebApplicationReport.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportField> ReportFields { get; set; }
    public DbSet<ReportEntry> ReportEntries { get; set; }
    public DbSet<ReportEntryValue> ReportEntryValues { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ReportEntryValue>()
            .HasOne(v => v.ReportField)
            .WithMany()
            .HasForeignKey(v => v.ReportFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ReportEntryValue>()
            .HasOne(v => v.ReportEntry)
            .WithMany(e => e.Values)
            .HasForeignKey(v => v.ReportEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
