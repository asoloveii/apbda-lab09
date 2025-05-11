using apbd_lab09.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;

namespace apbd_lab09.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString;
    private readonly IValidation _validator;

    public WarehouseService(IConfiguration configuration, IValidation validation)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _validator = validation;
    }
    
    public async Task<(bool IsSuccess, int Id, string ErrorMessage, int StatusCode)> AddProductToWarehouse(ProductWarehouseDTO request)
    {
        var conn = new SqlConnection(_connectionString);
        var transaction = conn.BeginTransaction();
        await conn.OpenAsync();
        
        // validate request
        var validation = await _validator.ValidateAllAsync(request, conn, transaction);
        
        // check validations
        if (!validation.IsValid)
            return (false, 0, validation.ErrorMessage, validation.StatusCode);
        
        try
        {
            // 1. Update Order FullfilledAt
            var updateOrderCmd = new SqlCommand("UPDATE [Order] SET FullfilledAt = @Now WHERE IdOrder = @Id", conn, transaction);
            updateOrderCmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            updateOrderCmd.Parameters.AddWithValue("@Id", validation.OrderId.Value);
            await updateOrderCmd.ExecuteNonQueryAsync();

            // 2. Insert into Product_Warehouse
            var insertCmd = new SqlCommand(@"
                INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                OUTPUT INSERTED.IdProductWarehouse
                VALUES (@WarehouseId, @ProductId, @OrderId, @Amount, @Price, @CreatedAt)", conn, transaction);

            insertCmd.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
            insertCmd.Parameters.AddWithValue("@ProductId", request.ProductId);
            insertCmd.Parameters.AddWithValue("@OrderId", validation.OrderId.Value);
            insertCmd.Parameters.AddWithValue("@Amount", request.Amount);
            insertCmd.Parameters.AddWithValue("@Price", validation.ProductPrice.Value * request.Amount);
            insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var newId = (int)await insertCmd.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return (true, newId, null, 200);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, 0, $"Internal error: {ex.Message}", 500);
        }
    }

    public async Task<(bool IsSuccess, int Id, string ErrorMessage, int StatusCode)> AddProductToWarehouseUsingProcedure(ProductWarehouseDTO request)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        try
        {
            using var cmd = new SqlCommand("AddProductToWarehouse", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdProduct", request.ProductId);
            cmd.Parameters.AddWithValue("@IdWarehouse", request.WarehouseId);
            cmd.Parameters.AddWithValue("@Amount", request.Amount);
            cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

            var outputParam = new SqlParameter("@IdProductWarehouse", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            await cmd.ExecuteNonQueryAsync();

            if (outputParam.Value == DBNull.Value)
                return (false, 0, "Stored procedure failed to return ID", 500);

            int resultId = (int)outputParam.Value;
            return (true, resultId, null, 200);
        }
        catch (SqlException sqlEx)
        {
            return (false, 0, $"SQL Error: {sqlEx.Message}", 500);
        }
        catch (Exception ex)
        {
            return (false, 0, $"Internal Error: {ex.Message}", 500);
        }
    }
}