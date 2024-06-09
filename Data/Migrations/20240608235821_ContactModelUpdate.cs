using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContactModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactUserEmail",
                table: "Contacts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactUserEmail",
                table: "Contacts");
        }
    }
}
