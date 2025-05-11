namespace apbd_lab09.Services;

using apbd_lab09.Models.DTOs;

public interface IWarehouseService
{
    Task<(bool IsSuccess, int Id, string ErrorMessage, int StatusCode)> AddProductToWarehouse(ProductWarehouseDTO request);
    Task<(bool IsSuccess, int Id, string ErrorMessage, int StatusCode)> AddProductToWarehouseUsingProcedure(ProductWarehouseDTO request);
}