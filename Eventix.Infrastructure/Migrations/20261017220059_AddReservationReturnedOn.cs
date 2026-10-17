using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationReturnedOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ReturnedOn",
                table: "Reservations",
                type: "date",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reservations_ReturnedOn_NotBeforeStart",
                table: "Reservations",
                sql: "\"ReturnedOn\" IS NULL OR \"ReturnedOn\" >= \"StartDate\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reservations_ReturnedOn_NotBeforeStart",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReturnedOn",
                table: "Reservations");
        }
    }
}
