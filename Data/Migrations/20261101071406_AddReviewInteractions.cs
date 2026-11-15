using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IS7024_GroupProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModeratedAt",
                table: "BookReviews",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationReason",
                table: "BookReviews",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                table: "BookReviews",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "BookReviews",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReviewComments",
                columns: table => new
                {
                    ReviewCommentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookReviewId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModerationStatus = table.Column<string>(type: "TEXT", nullable: false),
                    ModerationReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ModeratedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewComments", x => x.ReviewCommentId);
                    table.ForeignKey(
                        name: "FK_ReviewComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewComments_BookReviews_BookReviewId",
                        column: x => x.BookReviewId,
                        principalTable: "BookReviews",
                        principalColumn: "BookReviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewLikes",
                columns: table => new
                {
                    ReviewLikeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookReviewId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewLikes", x => x.ReviewLikeId);
                    table.ForeignKey(
                        name: "FK_ReviewLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewLikes_BookReviews_BookReviewId",
                        column: x => x.BookReviewId,
                        principalTable: "BookReviews",
                        principalColumn: "BookReviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComments_BookReviewId",
                table: "ReviewComments",
                column: "BookReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComments_UserId",
                table: "ReviewComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLikes_BookReviewId",
                table: "ReviewLikes",
                column: "BookReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLikes_UserId_BookReviewId",
                table: "ReviewLikes",
                columns: new[] { "UserId", "BookReviewId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewComments");

            migrationBuilder.DropTable(
                name: "ReviewLikes");

            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "BookReviews");

            migrationBuilder.DropColumn(
                name: "ModerationReason",
                table: "BookReviews");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "BookReviews");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "BookReviews");
        }
    }
}
