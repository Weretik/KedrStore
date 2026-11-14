namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>The kind of product a name describes (Python <c>product_kind</c>); it decides how sizes are read.</summary>
internal static class ProductKinds
{
    public static string Of(RecognitionProduct product, RecognitionDictionary dictionary)
    {
        var name = product.Name;
        var categoryId = product.CategoryId;
        bool Has(string pattern) => PythonRegex.Search(pattern, name) is not null;
        bool StartsWith(string pattern) => PythonRegex.MatchStart(pattern, name) is not null;

        if (StartsWith(@"(?:Робота|Ремонт|Доставка)\b"))
            return "service";
        if (categoryId is 6799 or 8719 || StartsWith(@"(?:Каталог|Буклет|Образцы|Рекламні)\b"))
            return "advertising";
        if (Has(@"Дверне\s+полотно|Каркасно-щитові\s+двері"))
            return "door_leaf";
        if (categoryId is { } id && dictionary.EntryCategoryIds.Contains(id) && StartsWith(@"Двері\b"))
            return "entrance_door";
        if (Has(@"Коробка|Лиштва|Добірна\s+дошка|Розширювач|Плінтус|профіль|Притворна\s+планка|Капітель|Стоєва|ПОРТАЛ"))
            return "trim";
        if (Has("Завіса|петля"))
        {
            if (Has("прихован"))
                return "concealed_hinge";
            if (Has("приварна|ввертна|проточена|точена"))
                return "round_hinge";
            if (categoryId is 6139 or 1707 || name.ToUpperInvariant().Contains("FLUSH", StringComparison.Ordinal)
                                           || Has(@"(?:75|100|125)\s*[*xх×]\s*(?:62|63|75)\b"))
                return "flat_hinge";
            return "other_hinge";
        }

        if (Has(@"ковпачок|\bCAP\b"))
            return "hinge_cap";
        if (NameObservationExtractor.IsSmart(product, dictionary))
            return "smart_cylinder";
        if (Has("циліндр|цилиндр|секрет"))
            return "cylinder";
        if (Has("вічко"))
            return "viewer";
        if (Has("дотягувач"))
            return "closer";
        if (Has("Ущільнювач"))
            return "seal";
        if (StartsWith(@"KEDR\s+Квадрат\b"))
            return "spindle";
        if (Has("ґвинт|гвинт|саморіз"))
            return "screw";
        if (Has("рейка"))
            return "rail";
        if (Has("шпінгалет"))
            return "bolt";
        if (Has("ручка|ручки"))
            return "handle";
        if (Has(@"замок|Корпус\s+замка"))
            return "lock";
        if (Has("Кутник|Пластина|Провушина|Підвіс|Кріпленння|Стріла"))
            return "metal_fitting";
        return "other";
    }
}
