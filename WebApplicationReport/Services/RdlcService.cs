using System.Data;
using Microsoft.Reporting.NETCore;
using DomainReport = WebApplicationReport.Models.Domain.Report;
using WebApplicationReport.Models.Domain;

namespace WebApplicationReport.Services;

public class RdlcService
{
    private readonly IWebHostEnvironment _env;

    public RdlcService(IWebHostEnvironment env)
    {
        _env = env;
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    public byte[] Render(DomainReport report, List<ReportField> fields,
                         List<ReportEntry> entries, string format)
    {
        var logoBase64 = LoadLogoBase64();
        var rdlcXml   = RdlcReportBuilder.Build(report, fields, logoBase64);
        var dataTable  = BuildDataTable(fields, entries);

        using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rdlcXml));
        var localReport = new LocalReport();
        localReport.LoadReportDefinition(ms);
        localReport.DataSources.Add(new ReportDataSource("ReportData", dataTable));

        return localReport.Render(format == "EXCELOPENXML" ? "EXCELOPENXML" : "PDF");
    }

    private string? LoadLogoBase64()
    {
        var path = Path.Combine(_env.WebRootPath, "images", "logo.png");
        return File.Exists(path) ? Convert.ToBase64String(File.ReadAllBytes(path)) : null;
    }

    private static DataTable BuildDataTable(List<ReportField> fields, List<ReportEntry> entries)
    {
        var dt = new DataTable("ReportData");

        foreach (var f in fields.OrderBy(f => f.Order))
            dt.Columns.Add($"F_{f.Id}", typeof(string));

        dt.Columns.Add("SubmittedBy", typeof(string));
        dt.Columns.Add("SubmittedAt", typeof(string));

        foreach (var entry in entries)
        {
            var row = dt.NewRow();
            foreach (var f in fields)
                row[$"F_{f.Id}"] = entry.Values.FirstOrDefault(v => v.ReportFieldId == f.Id)?.Value ?? "";
            row["SubmittedBy"] = entry.SubmittedByEmail;
            row["SubmittedAt"] = entry.SubmittedAt.ToString("yyyy-MM-dd HH:mm");
            dt.Rows.Add(row);
        }

        return dt;
    }
}
