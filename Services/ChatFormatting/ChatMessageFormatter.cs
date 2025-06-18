using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;

namespace WiseHR.Services.ChatFormatting;

public class ChatMessageFormatter : IChatMessageFormatter
{
    public string FormatGeneralMessageContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        content = content.Replace("\r\n", "\n").Trim();

        var formattedContent = Regex.Replace(content, @"\*\*(.*?)\*\*", "<strong>$1</strong>");

        formattedContent = Regex.Replace(formattedContent, @"\s+(\*|\-)\s+", "\n$1 ");

        var lines = formattedContent.Split('\n');
        var formattedLines = new List<string>();
        var inBulletList = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (inBulletList)
                {
                    formattedLines.Add("</div>");
                    inBulletList = false;
                }
                formattedLines.Add("<div class='wisechat-spacing'></div>");
                continue;
            }

            var trimmedLine = line.TrimStart();
            bool isBulletLine = Regex.IsMatch(trimmedLine, @"^(\-|\*)\s+.*", RegexOptions.None, TimeSpan.FromMilliseconds(100));

            if (isBulletLine)
            {
                if (!inBulletList)
                {
                    formattedLines.Add("<div class='wisechat-bullet-list'>");
                    inBulletList = true;
                }

                string contentAfterBullet = Regex.Replace(trimmedLine, @"^(\-|\*)\s+", "").TrimStart();

                string finalBulletContent = contentAfterBullet;
                if (!finalBulletContent.StartsWith("<strong>"))
                {
                    Match headingMatch = Regex.Match(contentAfterBullet, @"^([\w\s.,&;/\-]+?):", RegexOptions.None, TimeSpan.FromMilliseconds(100));
                    if (headingMatch.Success)
                    {
                        finalBulletContent = Regex.Replace(contentAfterBullet, @"^([\w\s.,&;/\-]+?):", "<strong>$1:</strong>", RegexOptions.None, TimeSpan.FromMilliseconds(100));
                    }
                }
                
                var isNested = line.Length - trimmedLine.Length >= 4;
                var bulletClass = isNested ? "wisechat-nested-bullet" : "wisechat-bullet";

                formattedLines.Add($"<div class='{bulletClass}'><span class='bullet'>•</span><span class='bullet-content'>{finalBulletContent}</span></div>");
            }
            else if (Regex.IsMatch(trimmedLine, @"^\d+\.", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
            {
                if (inBulletList)
                {
                    formattedLines.Add("</div>");
                    inBulletList = false;
                }
                string numberedContent = trimmedLine;
                formattedLines.Add($"<div class='wisechat-numbered-point'>{numberedContent}</div>");
            }
            else
            {
                if (inBulletList)
                {
                    formattedLines.Add("</div>");
                    inBulletList = false;
                }
                formattedLines.Add($"<div class='wisechat-text-line'>{trimmedLine}</div>");
            }
        }

        if (inBulletList)
        {
            formattedLines.Add("</div>");
        }

        return string.Join("", formattedLines);
    }

    public string FormatGenericTableContent(string content)
{
    var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(line => line.Trim())
                       .ToList();

    if (lines.Count == 1 && lines[0].Contains('|'))
    {
        // Single-row fallback: assume Field | Value
        var row = lines[0].Split('|')
                          .Select(cell => WebUtility.HtmlEncode(cell.Trim()))
                          .ToList();

        if (row.Count == 2)
        {
            var tableHtml = new StringBuilder();
            tableHtml.Append("<div class='wisechat-table-container'>");
            tableHtml.Append("<table class='wisechat-table'>");
                tableHtml.Append("<thead><tr><th>Field</th><th>Value</th></tr></thead>");
                tableHtml.Append("<tbody>");
            tableHtml.Append("<tr>");
                tableHtml.Append($"<td class='wisechat-field'>{row[0]}</td>");
                tableHtml.Append($"<td class='wisechat-value'>{row[1]}</td>");
            tableHtml.Append("</tr>");
            tableHtml.Append("</tbody></table></div>");
            return tableHtml.ToString();
        }
    }

    // Standard case
    if (lines.Count < 2 || lines[1].All(c => c == '-' || c == '•') == false)
    {
        return "<p>Invalid table format. Expected header row with separator.</p>";
    }

    var headers = lines[0].Split('|')
                         .Select(h => WebUtility.HtmlEncode(h.Trim()))
                         .ToList();

    var dataRows = new List<List<string>>();
    for (int i = 2; i < lines.Count; i++)
    {
        var rowValues = lines[i].Split('|')
                                .Select(cell => WebUtility.HtmlEncode(cell.Trim()))
                                .ToList();

        while (rowValues.Count < headers.Count)
        {
            rowValues.Add("");
        }
        if (rowValues.Count > headers.Count)
        {
            rowValues = rowValues.Take(headers.Count).ToList();
        }
        dataRows.Add(rowValues);
    }

    var fullTableHtml = new StringBuilder();
    fullTableHtml.Append("<div class='wisechat-table-container'>");
    fullTableHtml.Append("<table class='wisechat-table'>");
        fullTableHtml.Append("<thead><tr><th>Field</th><th>Value</th></tr></thead>");
        fullTableHtml.Append("<tbody>");

        // For each column, create a field-value pair
        for (int colIndex = 0; colIndex < headers.Count; colIndex++)
    {
            fullTableHtml.Append("<tr>");
            fullTableHtml.Append($"<td class='wisechat-field'>{headers[colIndex]}</td>");
            fullTableHtml.Append("<td class='wisechat-value'>");

            // Combine all values for this column
            var values = dataRows.Select(row => row[colIndex]).Where(v => !string.IsNullOrWhiteSpace(v));
            fullTableHtml.Append(string.Join("<br/>", values));
            
            fullTableHtml.Append("</td>");
        fullTableHtml.Append("</tr>");
    }

    fullTableHtml.Append("</tbody></table></div>");
    return fullTableHtml.ToString();
}

    public string FormatAssetTableContent(string content)
    {
        var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(line => line.Trim())
                           .ToList();

        var assets = new List<Dictionary<string, object?>>();
        Dictionary<string, object?>? currentAsset = null;
        List<string> instances = new List<string>();
        string? currentAssetType = null;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("Successfully executed")) continue;

            if (Regex.IsMatch(line, @"^[A-Za-z\s]+\s*\([\w\s-]+\)$", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
            {
                // Save previous asset
                if (currentAsset != null)
                {
                    if (instances.Any())
                        currentAsset["Instances"] = string.Join(", ", instances);
                    assets.Add(currentAsset);
                    instances = new List<string>();
                }
                currentAssetType = line.Trim();
                currentAsset = new Dictionary<string, object?>
                {
                    { "Asset Type", currentAssetType },
                    { "Tag Prefix", null },
                    { "Total Instances", null },
                    { "Instances", null }
                };
            }
            else if (line.StartsWith("Asset Tag Prefix:"))
            {
                var parts = line.Split(':');
                if (parts.Length >= 2 && currentAsset != null)
                {
                    currentAsset["Tag Prefix"] = parts[1].Trim();
                }
            }
            else if (line.StartsWith("Total Instances:"))
            {
                var parts = line.Split(':');
                if (parts.Length >= 2 && currentAsset != null)
                {
                    currentAsset["Total Instances"] = parts[1].Trim();
                }
            }
            else if (line.StartsWith("Instances:"))
            {
                // Just a marker, do nothing
            }
            else if (line.StartsWith("- ") && currentAsset != null)
            {
                instances.Add(line.Substring(2).Trim());
            }
        }

        // Add last asset
        if (currentAsset != null)
        {
            if (instances.Any())
                currentAsset["Instances"] = string.Join(", ", instances);
            assets.Add(currentAsset);
        }

        if (!assets.Any())
        {
            return $"<p>Could not format asset table. Raw content:</p><pre>{WebUtility.HtmlEncode(content)}</pre>";
        }

        var tableHtml = new StringBuilder();
        foreach (var asset in assets)
        {
            // Heading (bold)
            var assetType = asset.GetValueOrDefault("Asset Type") as string;
            if (!string.IsNullOrWhiteSpace(assetType))
            {
                tableHtml.Append($"<div style='font-weight:bold; font-size:1.1em; margin-top:1em; margin-bottom:0.5em;'>{WebUtility.HtmlEncode(assetType)}</div>");
            }
            tableHtml.Append("<div class='wisechat-asset-table-container'>");
            tableHtml.Append("<table class='wisechat-table'>");
            tableHtml.Append("<thead><tr><th>Field</th><th>Value</th></tr></thead><tbody>");
            foreach (var key in new[] { "Tag Prefix", "Total Instances", "Instances" })
            {
                var value = asset.GetValueOrDefault(key);
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    tableHtml.Append($"<tr><td>{WebUtility.HtmlEncode(key)}</td><td>{WebUtility.HtmlEncode(value.ToString())}</td></tr>");
                }
            }
            tableHtml.Append("</tbody></table></div>");
        }
        return tableHtml.ToString();
    }

    public string FormatJsonTableContent(string content)
    {
        string jsonToParse = content;
        string title = string.Empty;

        // Robustly extract JSON substring
        int firstBrace = content.IndexOf('{');
        int firstBracket = content.IndexOf('[');
        int jsonStart = -1;
        if (firstBrace != -1 && (firstBracket == -1 || firstBrace < firstBracket))
            jsonStart = firstBrace;
        else if (firstBracket != -1)
            jsonStart = firstBracket;

        if (jsonStart > 0)
        {
            title = content.Substring(0, jsonStart).Trim();
            jsonToParse = content.Substring(jsonStart).Trim();
        }
        else if (jsonStart == 0)
        {
            jsonToParse = content.Trim();
        }
        else
        {
            // No JSON found, fallback to previous logic
            jsonToParse = content;
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonToParse);
            var tableHtml = new StringBuilder();

            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var arrayElements = jsonDoc.RootElement.EnumerateArray().ToList();
                if (!arrayElements.Any())
                {
                    return "<p>The provided JSON array is empty.</p>";
                }

                if (arrayElements.Count == 1 && arrayElements.First().ValueKind == JsonValueKind.Object)
                {
                    var obj = arrayElements.First();
                    tableHtml.Append("<div class='wisechat-table-container'>");
                    tableHtml.Append("<table class='wisechat-table'>");
                    tableHtml.Append("<thead><tr><th>Field</th><th>Value</th></tr></thead><tbody>");
                    foreach (var prop in obj.EnumerateObject())
                    {
                        tableHtml.Append("<tr>");
                        tableHtml.Append($"<td>{WebUtility.HtmlEncode(prop.Name)}</td>");
                        tableHtml.Append($"<td>{WebUtility.HtmlEncode(prop.Value.ToString())}</td>");
                        tableHtml.Append("</tr>");
                    }
                    tableHtml.Append("</tbody></table></div>");
                    return tableHtml.ToString();
                }

                if (arrayElements.First().ValueKind != JsonValueKind.Object)
                {
                     return $"<p>JSON array does not contain objects. Raw content:</p><pre>{WebUtility.HtmlEncode(content)}</pre>";
                }

                var headers = arrayElements.First().EnumerateObject().Select(p => p.Name).ToList();

                tableHtml.Append("<div class='wisechat-table-container'>");
                tableHtml.Append("<table class='wisechat-table'>");
                tableHtml.Append("<thead><tr>");
                foreach (var header in headers)
                {
                    tableHtml.Append($"<th>{WebUtility.HtmlEncode(header)}</th>");
                }
                tableHtml.Append("</tr></thead>");

                tableHtml.Append("<tbody>");
                foreach (var element in arrayElements)
                {
                    tableHtml.Append("<tr>");
                    foreach (var header in headers)
                    {
                        var propValue = element.TryGetProperty(header, out var value) ? value.ToString() : string.Empty;
                        tableHtml.Append($"<td>{WebUtility.HtmlEncode(propValue)}</td>");
                    }
                    tableHtml.Append("</tr>");
                }
                tableHtml.Append("</tbody></table></div>");
            }
            else if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object)
            {
                tableHtml.Append("<div class='wisechat-table-container'>");
                tableHtml.Append("<table class='wisechat-table'>");
                tableHtml.Append("<thead><tr><th>Field</th><th>Value</th></tr></thead><tbody>");
                foreach (var prop in jsonDoc.RootElement.EnumerateObject())
                {
                    tableHtml.Append("<tr>");
                    tableHtml.Append($"<td>{WebUtility.HtmlEncode(prop.Name)}</td>");
                    tableHtml.Append($"<td>{WebUtility.HtmlEncode(prop.Value.ToString())}</td>");
                    tableHtml.Append("</tr>");
                }
                tableHtml.Append("</tbody></table></div>");
            }
            else
            {
                return $"<p>JSON content is not a valid object or array. Raw content:</p><pre>{WebUtility.HtmlEncode(content)}</pre>";
            }
            
            return tableHtml.ToString();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error in FormatJsonTableContent: {ex.Message}");
            return $"<p>The JSON content is incomplete or invalid. Raw content:</p><pre>{WebUtility.HtmlEncode(content)}</pre>";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General JSON formatting error in FormatJsonTableContent: {ex.Message}");
            return $"<p>An unexpected error occurred while formatting JSON. Raw content:</p><pre>{WebUtility.HtmlEncode(content)}</pre>";
        }
    }

    public ChatMessageTableType GetChatMessageTableType(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return ChatMessageTableType.None;

        string trimmedContent = content.Trim();

        int firstBrace = trimmedContent.IndexOf('{');
        int firstBracket = trimmedContent.IndexOf('[');
        
        int startIndex = -1;
        if (firstBrace != -1 && (firstBracket == -1 || firstBrace < firstBracket))
        {
            startIndex = firstBrace;
        }
        else if (firstBracket != -1)
        {
            startIndex = firstBracket;
        }

        if (startIndex != -1)
        {
            return ChatMessageTableType.Json;
        }

        var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(line => line.Trim())
                             .ToList();

        bool containsAssetKeywords = content.Contains("Asset Tag Prefix:") && content.Contains("Total Instances:");
        bool hasInstanceList = content.Contains("Instances:") && content.Contains("- ");

        if (containsAssetKeywords || hasInstanceList)
        {
            return ChatMessageTableType.Asset;
        }

        if (lines.Count < 1) return ChatMessageTableType.None;

        if (lines.Count >= 2 && lines[1].All(c => c == '-' || c == '•' || c == '|') && !string.IsNullOrWhiteSpace(lines[0]))
        {
            return ChatMessageTableType.Generic;
        }

        if (lines[0].Contains('|'))
        {
            return ChatMessageTableType.Generic;
        }

        return ChatMessageTableType.None;
    }
}
