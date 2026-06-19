namespace WebApplicationReport.Models.Domain;

public class Report
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedByUserId { get; set; } = string.Empty;

    public ICollection<ReportField> Fields { get; set; } = [];
    public ICollection<ReportEntry> Entries { get; set; } = [];
}
