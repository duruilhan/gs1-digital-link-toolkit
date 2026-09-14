using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gs1.DigitalLink.Resolver.Migrations
{
    /// <inheritdoc />
    public partial class EnforceTargetNullUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LinkTargets_LinkDefinitionId_LinkType_Language_MediaType",
                table: "LinkTargets");

            migrationBuilder.CreateIndex(
                name: "IX_LinkTargets_LinkDefinitionId_LinkType_Language_MediaType",
                table: "LinkTargets",
                columns: new[] { "LinkDefinitionId", "LinkType", "Language", "MediaType" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LinkTargets_LinkDefinitionId_LinkType_Language_MediaType",
                table: "LinkTargets");

            migrationBuilder.CreateIndex(
                name: "IX_LinkTargets_LinkDefinitionId_LinkType_Language_MediaType",
                table: "LinkTargets",
                columns: new[] { "LinkDefinitionId", "LinkType", "Language", "MediaType" },
                unique: true);
        }
    }
}
