using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingSystemMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderOptionItem_OptionItems_OptionItemId",
                table: "OrderOptionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderOptionItem_OrderItems_OrderItemId",
                table: "OrderOptionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderStatus_OrderStatusId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderStatus",
                table: "OrderStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderOptionItem",
                table: "OrderOptionItem");

            migrationBuilder.RenameTable(
                name: "OrderStatus",
                newName: "OrderStatuses");

            migrationBuilder.RenameTable(
                name: "OrderOptionItem",
                newName: "OrderOptionItems");

            migrationBuilder.RenameIndex(
                name: "IX_OrderOptionItem_OrderItemId",
                table: "OrderOptionItems",
                newName: "IX_OrderOptionItems_OrderItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderOptionItem_OptionItemId",
                table: "OrderOptionItems",
                newName: "IX_OrderOptionItems_OptionItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderStatuses",
                table: "OrderStatuses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderOptionItems",
                table: "OrderOptionItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderOptionItems_OptionItems_OptionItemId",
                table: "OrderOptionItems",
                column: "OptionItemId",
                principalTable: "OptionItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderOptionItems_OrderItems_OrderItemId",
                table: "OrderOptionItems",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderStatuses_OrderStatusId",
                table: "Orders",
                column: "OrderStatusId",
                principalTable: "OrderStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderOptionItems_OptionItems_OptionItemId",
                table: "OrderOptionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderOptionItems_OrderItems_OrderItemId",
                table: "OrderOptionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderStatuses_OrderStatusId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderStatuses",
                table: "OrderStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderOptionItems",
                table: "OrderOptionItems");

            migrationBuilder.RenameTable(
                name: "OrderStatuses",
                newName: "OrderStatus");

            migrationBuilder.RenameTable(
                name: "OrderOptionItems",
                newName: "OrderOptionItem");

            migrationBuilder.RenameIndex(
                name: "IX_OrderOptionItems_OrderItemId",
                table: "OrderOptionItem",
                newName: "IX_OrderOptionItem_OrderItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderOptionItems_OptionItemId",
                table: "OrderOptionItem",
                newName: "IX_OrderOptionItem_OptionItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderStatus",
                table: "OrderStatus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderOptionItem",
                table: "OrderOptionItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderOptionItem_OptionItems_OptionItemId",
                table: "OrderOptionItem",
                column: "OptionItemId",
                principalTable: "OptionItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderOptionItem_OrderItems_OrderItemId",
                table: "OrderOptionItem",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderStatus_OrderStatusId",
                table: "Orders",
                column: "OrderStatusId",
                principalTable: "OrderStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
