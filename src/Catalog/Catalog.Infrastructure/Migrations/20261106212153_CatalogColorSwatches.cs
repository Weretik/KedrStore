using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <summary>
    /// Fills the swatch color of plain and metal finishes of «Колір» (owner decision 2026-11-05: obvious colors get a
    /// hex, decors a texture from the media library, the rest stay text). Only values without a color or texture are
    /// filled, so nothing a person chose is overwritten; decors (codes DEC_*) are left for textures.
    /// </summary>
    public partial class CatalogColorSwatches : Migration
    {
        private static readonly (string Code, string Hex)[] Swatches =
        [
            ("CR", "#C9CED3"),      // Хром
            ("SN", "#B9B6AD"),      // Сатин
            ("AB", "#6E5233"),      // Антична бронза
            ("G", "#D4AF37"),       // Золото
            ("MBP", "#2B2B2B"),     // Чорний матовий
            ("BLACK", "#111111"),   // Чорний
            ("MWH", "#F2F2EE"),     // Білий матовий
            ("WH", "#FFFFFF"),      // Білий
            ("GRAFFIT", "#474A51"), // Графіт
            ("MCP", "#A9AEB3"),     // Хром матовий
            ("BROWN", "#6B4423"),   // Коричневий
            ("NI", "#B5B6B0"),      // Нікель
            ("BN", "#4A4A48"),      // Чорний нікель
            ("GREY", "#8E8E8E"),    // Сірий
            ("MSN", "#A8A59C"),     // Матовий нікель
            ("COPPER", "#B87333"),  // Мідний
            ("MACC", "#6F4E37"),    // Кава
            ("YELLOW", "#F2C200"),  // Жовтий
            ("RAL7024", "#474A50"), // Графітово-сірий
            ("SC", "#BFC1C2"),      // Шліфований алюміній
            ("ORB", "#3A2E26"),     // Чорний (ORB)
            ("RAL8017", "#44322D"), // Шоколадно-коричневий
            ("GM", "#C2A25A"),      // Матове золото
            ("SSG", "#CDB16E"),     // Сатинове золото
            ("AC", "#8A5A3B"),      // Антична мідь
            ("RED", "#C62828")      // Червоний
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (code, hex) in Swatches)
                migrationBuilder.Sql($"""
                    UPDATE "CatalogAttributeValues" AS v SET "SwatchHex" = '{hex}'
                    FROM "CatalogAttributes" AS a
                    WHERE v."AttributeId" = a."Id" AND a."Code" = 'color' AND v."Code" = '{code}'
                      AND v."SwatchHex" IS NULL AND v."SwatchMediaFileId" IS NULL;
                    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Clears only a color that is still the one filled here; a value a person changed keeps its own.
            foreach (var (code, hex) in Swatches)
                migrationBuilder.Sql($"""
                    UPDATE "CatalogAttributeValues" AS v SET "SwatchHex" = NULL
                    FROM "CatalogAttributes" AS a
                    WHERE v."AttributeId" = a."Id" AND a."Code" = 'color' AND v."Code" = '{code}'
                      AND v."SwatchHex" = '{hex}' AND NOT v."IsEditedManually";
                    """);
        }
    }
}
