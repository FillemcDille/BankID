
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BlazorApp4.Services
{
    public class LokalStorageService : IStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public LokalStorageService(IJSRuntime jSRuntime)
        {
            _jsRuntime = _jsRuntime;
        }
        


        public async Task<T?> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string?>("lokalStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _jsRuntime.InvokeVoidAsync("lokalStorage.setItem", key, json);
        }
    }
}
