namespace BlazorApp4.Interfaces
{
    /// <summary>
    /// Defines methods for asynchronously storing and retrieving data items by key.
    /// </summary>
    public interface IStorageService
    {    //Hämta
        Task SetItemAsync<T>(string key, T value);
        //Spara
        Task<T?> GetItemAsync<T>(string key);
    }
}
