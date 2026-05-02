using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Booking.Migrations
{
    /// <inheritdoc />
    public partial class _123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ListingTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Luxury Hotel" },
                    { 2, "Resort" },
                    { 3, "Business Hotel" },
                    { 4, "Boutique Hotel" },
                    { 5, "Budget Hotel" },
                    { 6, "Hostel" },
                    { 7, "Bed & Breakfast" },
                    { 8, "Apartment Hotel" },
                    { 9, "Extended Stay Hotel" },
                    { 10, "Eco-Friendly Hotel" },
                    { 11, "Historic Hotel" },
                    { 12, "Motel" },
                    { 13, "Capsule Hotel" },
                    { 14, "Beachfront Hotel" },
                    { 15, "Ski Resort" },
                    { 16, "Casino Hotel" },
                    { 17, "Waterpark Resort" },
                    { 18, "Wellness & Spa Hotel" },
                    { 19, "Guesthouse" },
                    { 20, "Floating Hotel" }
                });

            migrationBuilder.UpdateData(
                schema: "sec",
                table: "Users",
                keyColumn: "Id",
                keyValue: "8709e6d5-6c9e-442e-98d6-49d39f80014f",
                columns: new[] { "ConcurrencyStamp", "JoinDate", "PasswordHash" },
                values: new object[] { "8f569d50-58aa-4099-94b7-68a7cbd8ba41", new DateTime(2025, 2, 26, 23, 58, 26, 624, DateTimeKind.Local).AddTicks(6109), "AQAAAAIAAYagAAAAEDLsGpWQqGNUGY/bCKJFJQnGeUARgIEKmxiP0gGRLfq2lOULTF0m7rCfG3MU0TUt0Q==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ListingTypes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.UpdateData(
                schema: "sec",
                table: "Users",
                keyColumn: "Id",
                keyValue: "8709e6d5-6c9e-442e-98d6-49d39f80014f",
                columns: new[] { "ConcurrencyStamp", "JoinDate", "PasswordHash" },
                values: new object[] { "ff5d2ea9-f7fb-4445-b930-a6348963cb68", new DateTime(2025, 2, 26, 23, 55, 26, 152, DateTimeKind.Local).AddTicks(889), "AQAAAAIAAYagAAAAED0PklY97EV0se1VbVHMIomS0V4hpscnVI4xODJLziD5sobsw5UW4/B85ZxgTtwCdA==" });
        }
    }
}
