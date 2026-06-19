namespace WebApplicationReport.Models.Domain;

public class ReportField
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public Report Report { get; set; } = null!;

    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string? Options { get; set; } // comma-separated for Dropdown
}
