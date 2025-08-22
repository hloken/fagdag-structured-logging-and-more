namespace InventoryService.Contracts;

public record GetAllResponse(GetAllResponseItem[] Items);

public record GetAllResponseItem(string Name, int NumberOfItems);
