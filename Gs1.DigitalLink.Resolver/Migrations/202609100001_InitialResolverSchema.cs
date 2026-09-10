using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gs1.DigitalLink.Resolver.Migrations;

[DbContext(typeof(ResolverDbContext))]
[Migration("202609100001_InitialResolverSchema")]
public sealed class InitialResolverSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LinkDefinitions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CanonicalPath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_LinkDefinitions", item => item.Id));

        migrationBuilder.CreateTable(
            name: "LinkIdentifiers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LinkDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                Position = table.Column<int>(type: "integer", nullable: false),
                ApplicationIdentifier = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                Value = table.Column<string>(type: "character varying(90)", maxLength: 90, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LinkIdentifiers", item => item.Id);
                table.ForeignKey("FK_LinkIdentifiers_LinkDefinitions_LinkDefinitionId", item => item.LinkDefinitionId,
                    "LinkDefinitions", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "LinkTargets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LinkDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                LinkType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                Language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                MediaType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LinkTargets", item => item.Id);
                table.ForeignKey("FK_LinkTargets_LinkDefinitions_LinkDefinitionId", item => item.LinkDefinitionId,
                    "LinkDefinitions", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "LinkDefinitions",
            columns: ["Id", "CanonicalPath"],
            columnTypes: ["uuid", "character varying(2048)"],
            values: new object[,]
            {
                { Guid.Parse("10000000-0000-0000-0000-000000000001"), "/01/08690504080008" },
                { Guid.Parse("10000000-0000-0000-0000-000000000002"), "/01/08690504080008/10/LOT123" },
                { Guid.Parse("10000000-0000-0000-0000-000000000003"), "/414/8690123456789" }
            });

        migrationBuilder.InsertData(
            table: "LinkIdentifiers",
            columns: ["Id", "ApplicationIdentifier", "LinkDefinitionId", "Position", "Value"],
            columnTypes: ["uuid", "character varying(4)", "uuid", "integer", "character varying(90)"],
            values: new object[,]
            {
                { Guid.Parse("20000000-0000-0000-0000-000000000001"), "01", Guid.Parse("10000000-0000-0000-0000-000000000001"), 0, "08690504080008" },
                { Guid.Parse("20000000-0000-0000-0000-000000000002"), "01", Guid.Parse("10000000-0000-0000-0000-000000000002"), 0, "08690504080008" },
                { Guid.Parse("20000000-0000-0000-0000-000000000003"), "10", Guid.Parse("10000000-0000-0000-0000-000000000002"), 1, "LOT123" },
                { Guid.Parse("20000000-0000-0000-0000-000000000004"), "414", Guid.Parse("10000000-0000-0000-0000-000000000003"), 0, "8690123456789" }
            });

        migrationBuilder.InsertData(
            table: "LinkTargets",
            columns: ["Id", "IsDefault", "Language", "LinkDefinitionId", "LinkType", "MediaType", "Url"],
            columnTypes: ["uuid", "boolean", "character varying(16)", "uuid", "character varying(64)", "character varying(128)", "character varying(2048)"],
            values: new object[,]
            {
                { Guid.Parse("30000000-0000-0000-0000-000000000001"), false, "tr", Guid.Parse("10000000-0000-0000-0000-000000000001"), "gs1:pip", "text/html", "https://example.com/tr/products/08690504080008" },
                { Guid.Parse("30000000-0000-0000-0000-000000000002"), false, "en", Guid.Parse("10000000-0000-0000-0000-000000000001"), "gs1:pip", "text/html", "https://example.com/en/products/08690504080008" },
                { Guid.Parse("30000000-0000-0000-0000-000000000003"), true, null, Guid.Parse("10000000-0000-0000-0000-000000000001"), "gs1:defaultLink", "text/html", "https://example.com/products/08690504080008" },
                { Guid.Parse("30000000-0000-0000-0000-000000000004"), false, "tr", Guid.Parse("10000000-0000-0000-0000-000000000002"), "gs1:pip", "text/html", "https://example.com/tr/products/08690504080008/lots/LOT123" },
                { Guid.Parse("30000000-0000-0000-0000-000000000005"), true, null, Guid.Parse("10000000-0000-0000-0000-000000000003"), "gs1:defaultLink", "text/html", "https://example.com/locations/8690123456789" }
            });

        migrationBuilder.CreateIndex("IX_LinkDefinitions_CanonicalPath", "LinkDefinitions", "CanonicalPath", unique: true);
        migrationBuilder.CreateIndex("IX_LinkIdentifiers_ApplicationIdentifier_Value", "LinkIdentifiers", ["ApplicationIdentifier", "Value"]);
        migrationBuilder.CreateIndex("IX_LinkIdentifiers_LinkDefinitionId_Position", "LinkIdentifiers", ["LinkDefinitionId", "Position"], unique: true);
        migrationBuilder.CreateIndex("IX_LinkTargets_LinkDefinitionId", "LinkTargets", "LinkDefinitionId", unique: true, filter: "\"IsDefault\" = TRUE");
        migrationBuilder.CreateIndex("IX_LinkTargets_LinkDefinitionId_LinkType_Language", "LinkTargets", ["LinkDefinitionId", "LinkType", "Language"], unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("LinkIdentifiers");
        migrationBuilder.DropTable("LinkTargets");
        migrationBuilder.DropTable("LinkDefinitions");
    }
}
