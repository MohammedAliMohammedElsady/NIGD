using WebApplicationReport.Models.Domain;

namespace WebApplicationReport.Models.ViewModels;

public class ReportEntryViewModel
{
    public int ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public List<ReportEntryFieldViewModel> Fields { get; set; } = [];
}

public class ReportEntryFieldViewModel
{
    public int FieldId { get; set; }
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public List<string> Options { get; set; } = [];
    public string? Value { get; set; }
}
