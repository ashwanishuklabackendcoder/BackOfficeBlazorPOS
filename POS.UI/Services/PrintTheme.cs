using System.Net;
using System.Text;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public static class PrintTheme
    {
        public static string BuildDocumentStart(string title, bool landscape, string extraCss = "")
        {
            var orientation = landscape ? "A4 landscape" : "A4 portrait";
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset=\"utf-8\" />");
            sb.Append("<style>");
            sb.Append($"@page{{size:{orientation};margin:8mm;}}");
            sb.Append("body{font-family:Arial, sans-serif;color:#111;margin:0;padding:0;font-size:10px;line-height:1.35;}");
            sb.Append(".brand-header{display:flex;align-items:center;justify-content:space-between;gap:12px;padding-bottom:10px;margin-bottom:10px;border-bottom:1px solid #d8dde6;}");
            sb.Append(".brand-block{display:flex;align-items:center;gap:10px;min-width:0;}");
            sb.Append(".brand-logo{width:44px;height:44px;object-fit:contain;border-radius:6px;}");
            sb.Append(".brand-name{font-size:18px;font-weight:700;line-height:1.1;}");
            sb.Append(".doc-title{font-size:16px;font-weight:700;text-align:center;flex:1;}");
            sb.Append(".meta{font-size:10px;color:#555;line-height:1.45;margin-bottom:10px;}");
            sb.Append(".invoice-table,.report-table{width:100%;border-collapse:collapse;table-layout:fixed;}");
            sb.Append(".invoice-table th,.invoice-table td,.report-table th,.report-table td{border:1px solid #d5dbe5;padding:4px 6px;vertical-align:top;}");
            sb.Append(".invoice-table th,.report-table th{background:#f5f7fa;font-weight:700;}");
            sb.Append(".num{text-align:right;white-space:nowrap;}");
            sb.Append(".small{font-size:9px;}");
            sb.Append(".section{margin-top:10px;}");
            if (!string.IsNullOrWhiteSpace(extraCss))
                sb.Append(extraCss);
            sb.Append("</style></head><body>");
            return sb.ToString();
        }

        public static string BuildHeader(string title, CompanyBrandingDto? branding, string? subtitle = null)
        {
            var companyName = WebUtility.HtmlEncode(branding?.CompanyName?.Trim() ?? string.Empty);
            var companyLogoUrl = WebUtility.HtmlEncode(branding?.CompanyLogoUrl?.Trim() ?? string.Empty);
            var titleHtml = WebUtility.HtmlEncode(title);
            var subtitleHtml = string.IsNullOrWhiteSpace(subtitle) ? string.Empty : $"<div class=\"small text-muted\">{WebUtility.HtmlEncode(subtitle)}</div>";

            var sb = new StringBuilder();
            sb.Append("<div class=\"brand-header\">");
            sb.Append("<div class=\"brand-block\">");
            if (!string.IsNullOrWhiteSpace(companyLogoUrl))
                sb.Append($"<img class=\"brand-logo\" src=\"{companyLogoUrl}\" alt=\"Company logo\" />");
            sb.Append($"<div class=\"brand-name\">{(string.IsNullOrWhiteSpace(companyName) ? "POS.UI" : companyName)}</div>");
            sb.Append("</div>");
            sb.Append($"<div class=\"doc-title\">{titleHtml}{subtitleHtml}</div>");
            sb.Append("<div style=\"min-width:44px\"></div>");
            sb.Append("</div>");
            return sb.ToString();
        }

        public static string BuildDocumentEnd()
            => "</body></html>";
    }
}
