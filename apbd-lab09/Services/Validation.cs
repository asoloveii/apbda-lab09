using apbd_lab09.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_lab09.Services;

public class Validation : IValidation
{
    public async Task<bool> ProductExists(int productId, SqlConnection conn, SqlTransaction transaction)
    {
        var cmd = new SqlCommand("SELECT 1 FROM Product WHERE IdProduct = @Id", conn, transaction);
        cmd.Parameters.AddWithValue("@Id", productId);
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }

    public async Task<bool> WarehouseExists(int warehouseId, SqlConnection conn, SqlTransaction transaction)
    {
        var cmd = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @Id", conn, transaction);
        cmd.Parameters.AddWithValue("@Id", warehouseId);
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }
    
    public Task<bool> IsAmountValid(int amount)
    {
        return Task.FromResult(amount > 0);
    }

    public async Task<int?> GetMatchingOrderId(ProductWarehouseDTO request, SqlConnection conn, SqlTransaction transaction)
    {
        var cmd = new SqlCommand(@"
            SELECT TOP 1 IdOrder FROM [Order]
            WHERE IdProduct = @ProductId AND Amount = @Amount AND CreatedAt < @CreatedAt
            ORDER BY CreatedAt ASC", conn, transaction);

        cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

        var result = await cmd.ExecuteScalarAsync();
        return result != null ? (int?)result : null;
    }
    
    public async Task<bool> IsOrderFulfilled(int orderId, SqlConnection conn, SqlTransaction transaction)
    {
        var cmd = new SqlCommand("SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder", conn, transaction);
        cmd.Parameters.AddWithValue("@IdOrder", orderId);
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }
    
    public async Task<(bool IsValid, string ErrorMessage, int StatusCode, int? OrderId, decimal? ProductPrice)> ValidateAllAsync(ProductWarehouseDTO request, SqlConnection conn, SqlTransaction transaction)
    {
        if (!await ProductExists(request.ProductId, conn, transaction))
            return (false, "Product not found", 404, null, null);

        if (!await WarehouseExists(request.WarehouseId, conn, transaction))
            return (false, "Warehouse not found", 404, null, null);

        if (!await IsAmountValid(request.Amount))
            return (false, "Amount must be greater than 0", 400, null, null);

        var orderId = await GetMatchingOrderId(request, conn, transaction);
        if (orderId == null)
            return (false, "Matching order not found", 404, null, null);

        if (await IsOrderFulfilled(orderId.Value, conn, transaction))
            return (false, "Order already fulfilled", 400, null, null);

        // Retrieve product price
        var cmd = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @Id", conn, transaction);
        cmd.Parameters.AddWithValue("@Id", request.ProductId);
        var priceObj = await cmd.ExecuteScalarAsync();

        return (true, null, 200, orderId, priceObj is decimal price ? price : null);
    }
}