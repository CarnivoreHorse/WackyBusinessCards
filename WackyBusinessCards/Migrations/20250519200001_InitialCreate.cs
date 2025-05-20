using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WackyBusinessCards.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Company = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Website = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    TextColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    FontFamily = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BorderStyle = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BorderColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    BorderWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    BorderRadius = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SpecialEffect = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessCards", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BusinessCards",
                columns: new[] { "Id", "Address", "BackgroundColor", "BorderColor", "BorderRadius", "BorderStyle", "BorderWidth", "Company", "Email", "FontFamily", "ImageUrl", "Name", "Phone", "SpecialEffect", "TextColor", "Title", "Website" },
                values: new object[,]
                {
                    { 1, "1 Chocolate Factory Way, Loompaland", "#9b59b6", "#f1c40f", 15, "dashed", 5, "Wonka Industries", "willy@wonkachocolate.com", "'Comic Sans MS', cursive", "/images/chocolate.png", "Willy Wonka", "555-CHOCOLATE", "rotate", "#ffffff", "Chocolate Extraordinaire", "www.wonkachocolate.com" },
                    { 2, "Somewhere in the Caribbean", "#34495e", "#c0392b", 0, "double", 7, "Black Pearl Enterprises", "captain@blackpearl.sea", "'Pirata One', cursive", "/images/pirate.png", "Captain Jack Sparrow", "ARRRR-MATEY", "shadow", "#f1c40f", "Pirate Captain", "www.pirateslife.com" },
                    { 3, "Ottery St. Catchpole", "#3498db", "#e74c3c", 30, "dotted", 3, "The Quibbler", "luna@quibbler.wiz", "'Indie Flower', cursive", "/images/magic.png", "Luna Lovegood", "PATRONUS-123", "sparkle", "#ffffff", "Magizoologist", "www.thequibbler.wiz" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessCards");
        }
    }
}
