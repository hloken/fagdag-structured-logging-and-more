namespace InventoryService.Contracts;

public interface IDefineEmpty<T> where T:new()
{
    static T Empty { get; } = new();
}