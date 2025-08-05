using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateSessionCacheTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE [dbo].[SessionCache](
    [Id] nvarchar(449) NOT NULL PRIMARY KEY,
    [Value] varbinary(MAX) NOT NULL,
    [ExpiresAtTime] datetimeoffset NOT NULL,
    [SlidingExpirationInSeconds] bigint NULL,
    [AbsoluteExpiration] datetimeoffset NULL
);"); }


        

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
