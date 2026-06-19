using WebApplicationReport.Models.Domain;

namespace WebApplicationReport.Services;

public static class RdlcReportBuilder
{
    public static string Build(Report report, List<ReportField> fields, string? logoBase64)
    {
        var allFields = fields.OrderBy(f => f.Order).ToList();
        int colCount  = allFields.Count + 2; // +2 for SubmittedBy + SubmittedAt

        double pageW    = 17.0;
        double colW     = Math.Round(pageW / colCount, 2);
        string colWStr  = $"{colW}cm";

        var fieldDefs   = FieldsXml(allFields);
        var columns     = string.Concat(Enumerable.Repeat($"<TablixColumn><Width>{colWStr}</Width></TablixColumn>", colCount));
        var members     = string.Concat(Enumerable.Repeat("<TablixMember />", colCount));
        var headerCells = HeaderCells(allFields);
        var dataCells   = DataCells(allFields);
        var logoXml     = logoBase64 != null ? EmbeddedLogoXml(logoBase64) : "";
        var logoItem    = logoBase64 != null ? LogoImageXml() : "";

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Report xmlns=""http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition""
        xmlns:rd=""http://schemas.microsoft.com/SQLServer/reporting/reportdesigner"">
  {logoXml}
  <DataSources>
    <DataSource Name=""DS"">
      <ConnectionProperties>
        <DataProvider>System.Data.DataSet</DataProvider>
        <ConnectString>/* Local Connection */</ConnectString>
      </ConnectionProperties>
    </DataSource>
  </DataSources>
  <DataSets>
    <DataSet Name=""ReportData"">
      <Query>
        <DataSourceName>DS</DataSourceName>
        <CommandText>/* Local Query */</CommandText>
      </Query>
      <Fields>
        {fieldDefs}
        <Field Name=""SubmittedBy""><DataField>SubmittedBy</DataField><rd:TypeName>System.String</rd:TypeName></Field>
        <Field Name=""SubmittedAt""><DataField>SubmittedAt</DataField><rd:TypeName>System.String</rd:TypeName></Field>
      </Fields>
    </DataSet>
  </DataSets>
  <Body>
    <ReportItems>
      {logoItem}
      <Textbox Name=""InstituteName"">
        <Paragraphs><Paragraph><TextRuns><TextRun>
          <Value>المعهد القومي للحوكمة والتنمية المستدامة</Value>
          <Style><FontFamily>Tahoma</FontFamily><FontSize>13pt</FontSize><FontWeight>Bold</FontWeight><Color>#8B6914</Color></Style>
        </TextRun></TextRuns><Style><TextAlign>Center</TextAlign></Style></Paragraph></Paragraphs>
        <Top>0cm</Top><Left>0cm</Left><Width>{pageW}cm</Width><Height>0.9cm</Height>
        <Style><Border><Style>None</Style></Border></Style>
      </Textbox>
      <Textbox Name=""ReportTitle"">
        <Paragraphs><Paragraph><TextRuns><TextRun>
          <Value>{Esc(report.Name)}</Value>
          <Style><FontFamily>Tahoma</FontFamily><FontSize>14pt</FontSize><FontWeight>Bold</FontWeight><Color>#1a3c6e</Color></Style>
        </TextRun></TextRuns><Style><TextAlign>Center</TextAlign></Style></Paragraph></Paragraphs>
        <Top>1cm</Top><Left>0cm</Left><Width>{pageW}cm</Width><Height>0.9cm</Height>
        <Style><Border><Style>None</Style></Border></Style>
      </Textbox>
      <Textbox Name=""ReportDesc"">
        <Paragraphs><Paragraph><TextRuns><TextRun>
          <Value>{Esc(report.Description ?? "")}</Value>
          <Style><FontFamily>Tahoma</FontFamily><FontSize>9pt</FontSize><Color>#555555</Color></Style>
        </TextRun></TextRuns><Style><TextAlign>Center</TextAlign></Style></Paragraph></Paragraphs>
        <Top>2cm</Top><Left>0cm</Left><Width>{pageW}cm</Width><Height>0.5cm</Height>
        <Style><Border><Style>None</Style></Border></Style>
      </Textbox>
      <Textbox Name=""PrintDate"">
        <Paragraphs><Paragraph><TextRuns><TextRun>
          <Value>{DateTime.Now:yyyy-MM-dd HH:mm}</Value>
          <Style><FontFamily>Tahoma</FontFamily><FontSize>8pt</FontSize><Color>#888888</Color></Style>
        </TextRun></TextRuns><Style><TextAlign>Right</TextAlign></Style></Paragraph></Paragraphs>
        <Top>2cm</Top><Left>0cm</Left><Width>{pageW}cm</Width><Height>0.5cm</Height>
        <Style><Border><Style>None</Style></Border></Style>
      </Textbox>
      <Tablix Name=""DataTable"">
        <TablixBody>
          <TablixColumns>{columns}</TablixColumns>
          <TablixRows>
            <TablixRow>
              <Height>0.7cm</Height>
              <TablixCells>{headerCells}</TablixCells>
            </TablixRow>
            <TablixRow>
              <Height>0.6cm</Height>
              <TablixCells>{dataCells}</TablixCells>
            </TablixRow>
          </TablixRows>
        </TablixBody>
        <TablixColumnHierarchy><TablixMembers>{members}</TablixMembers></TablixColumnHierarchy>
        <TablixRowHierarchy>
          <TablixMembers>
            <TablixMember>
              <KeepWithGroup>After</KeepWithGroup>
              <RepeatOnNewPage>true</RepeatOnNewPage>
            </TablixMember>
            <TablixMember><Group Name=""Details"" /></TablixMember>
          </TablixMembers>
        </TablixRowHierarchy>
        <DataSetName>ReportData</DataSetName>
        <Top>2.8cm</Top><Left>0cm</Left><Width>{pageW}cm</Width>
        <Style><Border><Style>Solid</Style><Color>#1a3c6e</Color><Width>1pt</Width></Border></Style>
      </Tablix>
    </ReportItems>
    <Height>20cm</Height>
  </Body>
  <Page>
    <PageFooter>
      <PrintOnFirstPage>true</PrintOnFirstPage>
      <PrintOnLastPage>true</PrintOnLastPage>
      <ReportItems>
        <Textbox Name=""PageNum"">
          <Paragraphs><Paragraph><TextRuns><TextRun>
            <Value>=Globals!PageNumber &amp; "" / "" &amp; Globals!TotalPages</Value>
            <Style><FontFamily>Tahoma</FontFamily><FontSize>8pt</FontSize><Color>#888888</Color></Style>
          </TextRun></TextRuns><Style><TextAlign>Center</TextAlign></Style></Paragraph></Paragraphs>
          <Top>0.1cm</Top><Left>0cm</Left><Width>{pageW}cm</Width><Height>0.5cm</Height>
          <Style><Border><Style>None</Style></Border></Style>
        </Textbox>
      </ReportItems>
      <Height>0.7cm</Height>
    </PageFooter>
    <PageHeight>29.7cm</PageHeight>
    <PageWidth>21cm</PageWidth>
    <LeftMargin>2cm</LeftMargin>
    <RightMargin>2cm</RightMargin>
    <TopMargin>1cm</TopMargin>
    <BottomMargin>1cm</BottomMargin>
  </Page>
  <Width>{pageW}cm</Width>
  <Language>ar-EG</Language>
</Report>";
    }

    private static string FieldsXml(List<ReportField> fields)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var f in fields)
            sb.Append($@"<Field Name=""F_{f.Id}""><DataField>F_{f.Id}</DataField><rd:TypeName>System.String</rd:TypeName></Field>");
        return sb.ToString();
    }

    private static string HeaderCells(List<ReportField> fields)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var f in fields)
            sb.Append(HeaderCell($"TH_{f.Id}", Esc(f.Label)));
        sb.Append(HeaderCell("TH_By",  "Submitted By"));
        sb.Append(HeaderCell("TH_At",  "Date"));
        return sb.ToString();
    }

    private static string HeaderCell(string name, string label) => $@"
<TablixCell><CellContents>
  <Textbox Name=""{name}"">
    <Paragraphs><Paragraph><TextRuns><TextRun>
      <Value>{label}</Value>
      <Style><FontFamily>Tahoma</FontFamily><FontSize>9pt</FontSize><FontWeight>Bold</FontWeight><Color>White</Color></Style>
    </TextRun></TextRuns><Style><TextAlign>Center</TextAlign></Style></Paragraph></Paragraphs>
    <Style>
      <BackgroundColor>#1a3c6e</BackgroundColor>
      <VerticalAlign>Middle</VerticalAlign>
      <Border><Style>Solid</Style><Color>White</Color><Width>1pt</Width></Border>
    </Style>
  </Textbox>
</CellContents></TablixCell>";

    private static string DataCells(List<ReportField> fields)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var f in fields)
            sb.Append(DataCell($"TD_{f.Id}", $"=Fields!F_{f.Id}.Value"));
        sb.Append(DataCell("TD_By", "=Fields!SubmittedBy.Value"));
        sb.Append(DataCell("TD_At", "=Fields!SubmittedAt.Value"));
        return sb.ToString();
    }

    private static string DataCell(string name, string expr) => $@"
<TablixCell><CellContents>
  <Textbox Name=""{name}"">
    <Paragraphs><Paragraph><TextRuns><TextRun>
      <Value>{expr}</Value>
      <Style><FontFamily>Tahoma</FontFamily><FontSize>8pt</FontSize></Style>
    </TextRun></TextRuns><Style><TextAlign>Center</TextAlign></Style></Paragraph></Paragraphs>
    <Style>
      <BackgroundColor>=IIF(RowNumber(Nothing) Mod 2 = 0,""#eef2ff"",""White"")</BackgroundColor>
      <VerticalAlign>Middle</VerticalAlign>
      <Border><Style>Solid</Style><Color>#cccccc</Color></Border>
    </Style>
  </Textbox>
</CellContents></TablixCell>";

    private static string EmbeddedLogoXml(string base64) => $@"
<EmbeddedImages>
  <EmbeddedImage Name=""Logo"">
    <MIMEType>image/png</MIMEType>
    <ImageData>{base64}</ImageData>
  </EmbeddedImage>
</EmbeddedImages>";

    private static string LogoImageXml() => @"
<Image Name=""LogoImg"">
  <Source>Embedded</Source>
  <Value>Logo</Value>
  <Sizing>FitProportional</Sizing>
  <Top>0cm</Top><Left>0cm</Left><Width>2.5cm</Width><Height>2cm</Height>
  <Style><Border><Style>None</Style></Border></Style>
</Image>";

    private static string Esc(string s) =>
        System.Security.SecurityElement.Escape(s) ?? s;
}
