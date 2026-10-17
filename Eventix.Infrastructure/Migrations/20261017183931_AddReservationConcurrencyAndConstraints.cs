using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationConcurrencyAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "Reservations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status_EndDate",
                table: "Reservations",
                columns: new[] { "Status", "EndDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reservations_DateRange",
                table: "Reservations",
                sql: "\"EndDate\" >= \"StartDate\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReservationItems_Quantity_Positive",
                table: "ReservationItems",
                sql: "\"Quantity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DailyStocks_ReservedQuantity_NonNegative",
                table: "DailyStocks",
                sql: "\"ReservedQuantity\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_Status_EndDate",
                table: "Reservations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reservations_DateRange",
                table: "Reservations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReservationItems_Quantity_Positive",
                table: "ReservationItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DailyStocks_ReservedQuantity_NonNegative",
                table: "DailyStocks");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Reservations");
        }
    }
}
