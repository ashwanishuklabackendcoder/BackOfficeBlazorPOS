using System.IO.Compression;
using System.Net;
using System.Text;
using System.Security;
using BackOfficeBlazor.Shared.DTOs;
using POS.UI.Services;

namespace POS.UI.Shared
{
    public static class GridExportBuilder
    {
        private const string SpreadsheetContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public static string SpreadsheetMimeType => SpreadsheetContentType;

        public static string BuildPrintHtml(
            string title,
            IReadOnlyList<string> headers,
            IReadOnlyList<IReadOnlyList<string>> rows)
            => BuildBrandedPrintHtml(title, null, headers, rows);

        public static string BuildBrandedPrintHtml(
            string title,
            CompanyBrandingDto? branding,
            IReadOnlyList<string> headers,
            IReadOnlyList<IReadOnlyList<string>> rows,
            string? subtitle = null,
            bool landscape = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine(PrintTheme.BuildDocumentStart(title, landscape));
            sb.AppendLine(PrintTheme.BuildHeader(title, branding, subtitle ?? $"Exported {DateTime.Now:yyyy-MM-dd HH:mm}"));
            sb.AppendLine("<table class=\"report-table\">");
            sb.AppendLine("<thead><tr>");

            foreach (var header in headers)
            {
                sb.Append("<th>");
                sb.Append(EscapeHtml(header));
                sb.AppendLine("</th>");
            }

            sb.AppendLine("</tr></thead>");
            sb.AppendLine("<tbody>");

            if (rows.Count == 0)
            {
                sb.AppendLine($"<tr><td colspan=\"{Math.Max(headers.Count, 1)}\">No records found.</td></tr>");
            }
            else
            {
                foreach (var row in rows)
                {
                    sb.AppendLine("<tr>");
                    foreach (var cell in row)
                    {
                        sb.Append("<td>");
                        sb.Append(EscapeHtml(cell));
                        sb.AppendLine("</td>");
                    }
                    sb.AppendLine("</tr>");
                }
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine(PrintTheme.BuildDocumentEnd());
            return sb.ToString();
        }

        public static byte[] BuildWorkbookBytes(
            string sheetName,
            IReadOnlyList<string> headers,
            IReadOnlyList<IReadOnlyList<string>> rows)
        {
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                WriteEntry(archive, "[Content_Types].xml", BuildContentTypes());
                WriteEntry(archive, "_rels/.rels", BuildRootRels());
                WriteEntry(archive, "xl/workbook.xml", BuildWorkbookXml(sheetName));
                WriteEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRels());
                WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildSheetXml(headers, rows));
            }

            return stream.ToArray();
        }

        private static void WriteEntry(ZipArchive archive, string path, string content)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream, new UTF8Encoding(false));
            writer.Write(content);
        }

        private static string BuildContentTypes() =>
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
              <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
            </Types>
            """;

        private static string BuildRootRels() =>
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
            </Relationships>
            """;

        private static string BuildWorkbookXml(string sheetName) =>
            $"""
             <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
             <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
               <sheets>
                 <sheet name="{EscapeXml(SanitizeSheetName(sheetName))}" sheetId="1" r:id="rId1"/>
               </sheets>
             </workbook>
             """;

        private static string BuildWorkbookRels() =>
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
            </Relationships>
            """;

        private static string BuildSheetXml(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var lastCol = ColumnName(Math.Max(headers.Count, 1));
            var lastRow = rows.Count + 1;
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            xml.AppendLine("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            xml.AppendLine($"  <dimension ref=\"A1:{lastCol}{lastRow}\"/>");
            xml.AppendLine("  <sheetViews><sheetView workbookViewId=\"0\"/></sheetViews>");
            xml.AppendLine("  <sheetFormatPr defaultRowHeight=\"15\"/>");
            xml.AppendLine("  <sheetData>");

            AppendRow(xml, 1, headers.Select(h => h ?? string.Empty).ToList());

            var rowNumber = 2;
            foreach (var row in rows)
            {
                AppendRow(xml, rowNumber++, row.Select(c => c ?? string.Empty).ToList());
            }

            xml.AppendLine("  </sheetData>");
            xml.AppendLine("</worksheet>");
            return xml.ToString();
        }

        private static void AppendRow(StringBuilder xml, int rowNumber, IReadOnlyList<string> cells)
        {
            xml.AppendLine($"    <row r=\"{rowNumber}\">");
            for (var i = 0; i < cells.Count; i++)
            {
                var cellRef = $"{ColumnName(i + 1)}{rowNumber}";
                xml.Append("      <c r=\"");
                xml.Append(cellRef);
                xml.Append("\" t=\"inlineStr\"><is><t>");
                xml.Append(EscapeXml(cells[i]));
                xml.AppendLine("</t></is></c>");
            }
            xml.AppendLine("    </row>");
        }

        private static string ColumnName(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = string.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private static string SanitizeSheetName(string? name)
        {
            var cleaned = string.IsNullOrWhiteSpace(name) ? "Sheet1" : name.Trim();
            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };

            foreach (var invalid in invalidChars)
            {
                cleaned = cleaned.Replace(invalid, ' ');
            }

            cleaned = cleaned.Trim();
            if (cleaned.Length == 0)
                cleaned = "Sheet1";

            return cleaned.Length > 31 ? cleaned[..31] : cleaned;
        }

        private static string EscapeXml(string value)
            => SecurityElement.Escape(value) ?? string.Empty;

        private static string EscapeHtml(string value)
            => SecurityElement.Escape(value) ?? string.Empty;
    }
}
