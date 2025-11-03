using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BlazorApp4.Services
{
     /// <summary>
     /// Provides methods for storing and retrieving data in the browser's local storage using JavaScript interop.
     /// </summary>
     /// <remarks>This service enables Blazor applications to persist data on the client side by interacting
     /// with the browser's localStorage API. Data is serialized to JSON before storage and deserialized upon retrieval.
     /// The service is typically used for caching user preferences, session data, or other information that should
     /// persist across browser sessions.</remarks>
    public class LokalStorageService : IStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        
        /// <summary>
        /// Initializes a new instance of the LokalStorageService class using the specified JavaScript runtime.
        /// </summary>
        /// <remarks>Use this constructor when you need to interact with browser local storage via
        /// JavaScript interop in a Blazor application.</remarks>
        /// <param name="jsRuntime">The JavaScript runtime interface used to invoke JavaScript functions from .NET code.</param>
        public LokalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Asynchronously stores the specified value in the browser's local storage under the given key.
        /// </summary>
        /// <remarks>If a value already exists for the specified key, it will be overwritten. The value is
        /// serialized to JSON before being stored. This method does not perform validation on the value's
        /// serializability; serialization errors will result in exceptions.</remarks>
        /// <typeparam name="T">The type of the value to store in local storage. Must be serializable to JSON.</typeparam>
        /// <param name="key">The key under which the value will be stored in local storage. Cannot be null or empty.</param>
        /// <param name="value">The value to store in local storage. Must be serializable to JSON.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }

        /// <summary>
        /// Asynchronously retrieves and deserializes an item from browser local storage using the specified key.
        /// </summary>
        /// <remarks>If the specified key does not exist or the stored value is empty, the method returns
        /// <see langword="null"/>. The deserialization uses the configured JSON options, which may affect how the data
        /// is interpreted. This method is typically used in Blazor applications to persist and retrieve state between
        /// browser sessions.</remarks>
        /// <typeparam name="T">The type to which the stored JSON value will be deserialized.</typeparam>
        /// <param name="key">The key associated with the item in local storage. Cannot be null or empty.</param>
        /// <returns>A value of type <typeparamref name="T"/> if the item exists and can be deserialized; otherwise, <see
        /// langword="null"/>.</returns>
        public async Task<T?> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
    }
}
