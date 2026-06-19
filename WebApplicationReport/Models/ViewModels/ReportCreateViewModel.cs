using WebApplicationReport.Models.Domain;

namespace WebApplicationReport.Models.ViewModels;

public class ReportCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ReportFieldInputViewModel> Fields { get; set; } = [];
}

public class ReportFieldInputViewModel
{
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string? Options { get; set; }
}
