# Catalog attribute input files

Recognizer output for the [catalog attributes SDD](../sdd/specs/catalog/006-attributes-variants-filters/README.md), received from the owner on 2026-11-05. The files are data, not rules: the import reads them, and the parser test compares the .NET parser with them. `dictionaries.json` is also embedded in the application, so the recognizer after a 1C sync uses the committed version.

| File | What it is |
| --- | --- |
| `products_attributes.csv` | One row per product (2330): base name, size, colors, side, other attributes (`extra_attributes` JSON), `needs_review` and the reason. `null` means empty. |
| `dictionaries.json` | Attribute labels, canonical values, and spellings (entries) with their status; `selected_by=assistant` marks decodings the owner has not confirmed. |
| `extract_attributes.py` | The Python recognizer (v2.0) that produced the CSV; the reference for the .NET port. |
| `category_report.csv`, `base_name_examples.csv` | Summary per category and examples of base names before and after the last revision. |

Load into a database (dry run first; production only after the owner's «да»):

```bash
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs.csproj -- --job=import-catalog-attributes \
  --dictionary=docs/catalog-attributes/dictionaries.json --csv=docs/catalog-attributes/products_attributes.csv [--apply]
```
