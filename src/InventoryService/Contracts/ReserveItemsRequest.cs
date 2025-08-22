namespace InventoryService.Contracts;

public record struct ReserveItemsRequest(Guid OrderId, string ItemName, int NumberOfItems);