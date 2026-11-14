using System.Text;

namespace Catalog.Application.Features.Attributes.Shared.Import;

/// <summary>A small RFC 4180 reader: quoted fields, doubled quotes, and line breaks inside quotes.</summary>
public static class CsvText
{
    public static IEnumerable<(int LineNumber, IReadOnlyList<string> Fields)> ReadRecords(TextReader reader)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var line = 1;
        var recordLine = 1;
        int current;

        while ((current = reader.Read()) != -1)
        {
            var character = (char)current;
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (reader.Peek() == '"')
                    {
                        field.Append('"');
                        reader.Read();
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    if (character == '\n')
                        line++;
                    field.Append(character);
                }

                continue;
            }

            switch (character)
            {
                case '"':
                    inQuotes = true;
                    break;
                case ',':
                    fields.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    fields.Add(field.ToString());
                    field.Clear();
                    yield return (recordLine, fields);
                    fields = [];
                    line++;
                    recordLine = line;
                    break;
                default:
                    field.Append(character);
                    break;
            }
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            yield return (recordLine, fields);
        }
    }
}
