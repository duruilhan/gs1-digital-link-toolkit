using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gs1.DigitalLink.Resolver.Migrations
{
    /// <inheritdoc />
    public partial class InitialResolverSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LinkDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CanonicalPath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkDefinitions", x => x.Id);
                });

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
                    table.PrimaryKey("PK_LinkIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkIdentifiers_LinkDefinitions_LinkDefinitionId",
                        column: x => x.LinkDefinitionId,
                        principalTable: "LinkDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.PrimaryKey("PK_LinkTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkTargets_LinkDefinitions_LinkDefinitionId",
                        column: x => x.LinkDefinitionId,
                        principalTable: "LinkDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LinkDefinitions",
                columns: new[] { "Id", "CanonicalPath" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "/01/08690504080008" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "/01/08690504080008/10/LOT123" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "/414/8690123456789" }
                });

            migrationBuilder.InsertData(
                table: "LinkIdentifiers",
                columns: new[] { "Id", "ApplicationIdentifier", "LinkDefinitionId", "Position", "Value" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "01", new Guid("10000000-0000-0000-0000-000000000001"), 0, "08690504080008" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "01", new Guid("10000000-0000-0000-0000-000000000002"), 0, "08690504080008" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "10", new Guid("10000000-0000-0000-0000-000000000002"), 1, "LOT123" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "414", new Guid("10000000-0000-0000-0000-000000000003"), 0, "8690123456789" }
                });

            migrationBuilder.InsertData(
                table: "LinkTargets",
                columns: new[] { "Id", "IsDefault", "Language", "LinkDefinitionId", "LinkType", "MediaType", "Url" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), false, "tr", new Guid("10000000-0000-0000-0000-000000000001"), "gs1:pip", "text/html", "https://example.com/tr/products/08690504080008" },
                    { new Guid("30000000-0000-0000-0000-000000000002"), false, "en", new Guid("10000000-0000-0000-0000-000000000001"), "gs1:pip", "text/html", "https://example.com/en/products/08690504080008" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), true, null, new Guid("10000000-0000-0000-0000-000000000001"), "gs1:defaultLink", "text/html", "https://example.com/products/08690504080008" },
                    { new Guid("30000000-0000-0000-0000-000000000004"), false, "tr", new Guid("10000000-0000-0000-0000-000000000002"), "gs1:pip", "text/html", "https://example.com/tr/products/08690504080008/lots/LOT123" },
                    { new Guid("30000000-0000-0000-0000-000000000005"), true, null, new Guid("10000000-0000-0000-0000-000000000003"), "gs1:defaultLink", "text/html", "https://example.com/locations/8690123456789" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinkDefinitions_CanonicalPath",
                table: "LinkDefinitions",
                column: "CanonicalPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinkIdentifiers_ApplicationIdentifier_Value",
                table: "LinkIdentifiers",
                columns: new[] { "ApplicationIdentifier", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_LinkIdentifiers_LinkDefinitionId_Position",
                table: "LinkIdentifiers",
                columns: new[] { "LinkDefinitionId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinkTargets_LinkDefinitionId",
                table: "LinkTargets",
                column: "LinkDefinitionId",
                unique: true,
                filter: "\"IsDefault\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_LinkTargets_LinkDefinitionId_LinkType_Language_MediaType",
                table: "LinkTargets",
                columns: new[] { "LinkDefinitionId", "LinkType", "Language", "MediaType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkIdentifiers");

            migrationBuilder.DropTable(
                name: "LinkTargets");

            migrationBuilder.DropTable(
                name: "LinkDefinitions");
        }
    }
}
