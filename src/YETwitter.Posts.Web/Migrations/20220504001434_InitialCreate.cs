using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YETwitter.Posts.Web.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    text = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "appeals",
                columns: table => new
                {
                    value = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appeals", x => new { x.post_id, x.value });
                    table.ForeignKey(
                        name: "FK_appeals_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hashtags",
                columns: table => new
                {
                    value = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hashtags", x => new { x.post_id, x.value });
                    table.ForeignKey(
                        name: "FK_hashtags_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appeals");

            migrationBuilder.DropTable(
                name: "hashtags");

            migrationBuilder.DropTable(
                name: "posts");
        }
    }
}
