using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Migrations
{
    /// <inheritdoc />
    public partial class _321 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "sec",
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "86343a28-20ab-450e-b401-5fa16c073688", "8709e6d5-6c9e-442e-98d6-49d39f80014f" });

            migrationBuilder.UpdateData(
                schema: "sec",
                table: "Users",
                keyColumn: "Id",
                keyValue: "8709e6d5-6c9e-442e-98d6-49d39f80014f",
                columns: new[] { "ConcurrencyStamp", "JoinDate", "PasswordHash" },
                values: new object[] { "b6d80490-39cb-4c11-8dc8-f0df10ad9b4b", new DateTime(2025, 2, 27, 0, 10, 55, 620, DateTimeKind.Local).AddTicks(5665), "AQAAAAIAAYagAAAAEHEcb9jsoRFVRnTsqtPMJwj+BDCxG731pUBWcx/0NHvLJh4xuKplPlw+e+OGFz3hIg==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "sec",
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "86343a28-20ab-450e-b401-5fa16c073688", "8709e6d5-6c9e-442e-98d6-49d39f80014f" });

            migrationBuilder.UpdateData(
                schema: "sec",
                table: "Users",
                keyColumn: "Id",
                keyValue: "8709e6d5-6c9e-442e-98d6-49d39f80014f",
                columns: new[] { "ConcurrencyStamp", "JoinDate", "PasswordHash" },
                values: new object[] { "8f569d50-58aa-4099-94b7-68a7cbd8ba41", new DateTime(2025, 2, 26, 23, 58, 26, 624, DateTimeKind.Local).AddTicks(6109), "AQAAAAIAAYagAAAAEDLsGpWQqGNUGY/bCKJFJQnGeUARgIEKmxiP0gGRLfq2lOULTF0m7rCfG3MU0TUt0Q==" });
        }
    }
}
