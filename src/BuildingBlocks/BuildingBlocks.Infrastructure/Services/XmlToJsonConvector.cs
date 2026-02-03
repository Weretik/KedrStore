using System.Text;
using System.Xml.Linq;
using Catalog.Application.Contracts.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BuildingBlocks.Infrastructure.Services;

public sealed class XmlToJsonConvector(ILogger<XmlToJsonConvector> logger) : IXmlToJsonConvector
{
    public Task<Stream> CreateJsonStreamAsync(Stream xmlStream, CancellationToken cancellationToken)
    {
        xmlStream.Position = 0;
        string xmlRaw = Encoding.UTF8.GetString(((MemoryStream)xmlStream).ToArray());

        xmlRaw = xmlRaw.Replace("encoding=\"utf8\"", "encoding=\"utf-8\"", StringComparison.OrdinalIgnoreCase);
        xmlRaw = xmlRaw.Replace("<Свойства не назначены>", "");

        XDocument xdoc;

        try
        {
            xdoc = XDocument.Parse(xmlRaw, LoadOptions.PreserveWhitespace);
        }
        catch
        {
            xmlRaw = "<root>" + xmlRaw + "</root>";
            xdoc = XDocument.Parse(xmlRaw, LoadOptions.PreserveWhitespace);
        }

        RemoveWhitespaceNodes(xdoc.Root!);

        var json = JsonConvert.SerializeXNode(xdoc, Newtonsoft.Json.Formatting.None);

        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(jsonBytes);
        stream.Position = 0;

        logger.LogInformation("XML → JSON Stream generated. Size: {Size} bytes", jsonBytes.Length);

        return Task.FromResult<Stream>(stream);
    }

    private static void RemoveWhitespaceNodes(XElement element)
    {
        foreach (var node in element.Nodes().ToList())
        {
            if (node is XText txt && string.IsNullOrWhiteSpace(txt.Value))
                node.Remove();
            else if (node is XElement child)
                RemoveWhitespaceNodes(child);
        }
    }
}
