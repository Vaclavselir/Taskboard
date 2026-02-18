using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Tasks', 'RowVersion') IS NOT NULL
                    ALTER TABLE dbo.Tasks DROP COLUMN RowVersion;

                ALTER TABLE dbo.Tasks ADD RowVersion rowversion NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Tasks', 'RowVersion') IS NOT NULL
                    ALTER TABLE dbo.Tasks DROP COLUMN RowVersion;
            ");
        }
    }
}
