namespace BlazorApp4.Interfaces
{
    /// <summary>
    /// Defines methods for asynchronously storing and retrieving data items by key.
    /// </summary>
    public interface IStorageService
    {    
        /// <summary>
        /// Asynchronously sets the value associated with the specified key in the underlying storage.
        /// </summary>
        /// <typeparam name="T">The type of the value to store.</typeparam>
        /// <param name="key">The key with which the value will be associated. Cannot be null or empty.</param>
        /// <param name="value">The value to store. May be null if the storage implementation supports null values.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        
        Task SetItemAsync<T>(string key, T value);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T?> GetItemAsync<T>(string key);
    }
}
