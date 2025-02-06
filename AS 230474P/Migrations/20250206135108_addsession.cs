using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS_230474P.Migrations
{
    /// <inheritdoc />
    public partial class addsession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionToken",
                schema: "dbo",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionToken",
                schema: "dbo",
                table: "Registrations");
        }
    }
}
