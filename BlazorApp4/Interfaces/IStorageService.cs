namespace BlazorApp4.Interfaces
{
    public interface IStorageService
    {    //Hämta
        Task SetItemAsync<T>(string key, T value);
        //Spara
        Task<T?> GetItemAsync<T>(string key);
    }
}
