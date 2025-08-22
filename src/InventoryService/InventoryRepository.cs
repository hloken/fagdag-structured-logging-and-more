namespace InventoryService;

public class InventoryRepository
{
    public Dictionary<string, int> Items { get; init; } = new();
  
    public bool ReserveItems(string productName, int quantity)
    {
        var productQuantityInInventory = Items[productName];
        
        if (productQuantityInInventory >= quantity)
        {
            Items[productName] -= quantity;

            return true;
        }

        return false;
    }
}
