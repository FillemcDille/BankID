using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BlazorApp4.Services
{
    /// <summary>
    /// Provides simple methods for storing and retrieving data in the browser's local storage.
    /// </summary>
    /// <remarks>
    /// Data is serialized to JSON before being stored and deserialized upon retrieval.
    /// Useful for persisting user preferences or cached data between sessions.
    /// </remarks>
    public class LokalStorageService : IStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Initializes a new instance of the <see cref="LokalStorageService"/> class.
        /// </summary>
        public LokalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Saves a value to local storage under the specified key.
        /// </summary>
        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }

        /// <summary>
        /// Retrieves a value from local storage and deserializes it to the specified type.
        /// </summary>
        public async Task<T?> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
    }
}
