namespace BlazorApp4.Interfaces
{
    /// <summary>
    /// Defines methods for asynchronously storing and retrieving data items by key.
    /// </summary>
    public interface IStorageService
    {
        //Stores a value asynchronously under the given key
        Task SetItemAsync<T>(string key, T value);

        // Asynchronously retrieves an item of the specified type associated with the given key.
        Task<T?> GetItemAsync<T>(string key);
    }
}
