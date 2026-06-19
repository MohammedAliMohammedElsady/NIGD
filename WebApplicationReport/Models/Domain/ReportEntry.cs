namespace WebApplicationReport.Models.Domain;

public class ReportEntry
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public Report Report { get; set; } = null!;

    public string SubmittedByUserId { get; set; } = string.Empty;
    public string SubmittedByEmail { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.Now;

    public bool IsDeleted { get; set; } = false;

    public ICollection<ReportEntryValue> Values { get; set; } = [];
}
