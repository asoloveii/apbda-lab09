namespace apbd_lab09.Models;

public class ProductWarehouse
{
    public int ProductWarehouseId { get; set; }
    public int WareHouseId { get; set; }
    public int ProductId { get; set; }
    public int OrderId { get; set; }
    public int Amount { get; set; }
    public int Price { get; set; }
    public DateTime CreatedAt { get; set; }
}