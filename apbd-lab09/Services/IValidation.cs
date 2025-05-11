using apbd_lab09.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_lab09.Services;

public interface IValidation
{
    Task<bool> ProductExists(int productId, SqlConnection conn, SqlTransaction transaction);
    Task<bool> WarehouseExists(int warehouseId, SqlConnection conn, SqlTransaction transaction);
    Task<bool> IsAmountValid(int amount);
    Task<int?> GetMatchingOrderId(ProductWarehouseDTO request, SqlConnection conn, SqlTransaction transaction);
    Task<bool> IsOrderFulfilled(int orderId, SqlConnection conn, SqlTransaction transaction);
    Task<(bool IsValid, string ErrorMessage, int StatusCode, int? OrderId, decimal? ProductPrice)> ValidateAllAsync(ProductWarehouseDTO request, SqlConnection conn, SqlTransaction transaction);
}
