namespace WebApplicationReport.Models.Domain;

public class ReportEntryValue
{
    public int Id { get; set; }
    public int ReportEntryId { get; set; }
    public ReportEntry ReportEntry { get; set; } = null!;

    public int ReportFieldId { get; set; }
    public ReportField ReportField { get; set; } = null!;

    public string? Value { get; set; }
}
