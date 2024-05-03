using System.Text;
using System.Text.Json;
using AuthModule.Shared.Models;

namespace AuthModule.Client;

public sealed class HttpRequester: IDisposable
{
    private readonly HttpClient _httpClient;

    public HttpRequester()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5118/api/account/");
    }

    public async Task<EncryptedMessageModel> GetMessageAsync(CreateEncryptedMessageModel createEncryptedMessageModel) =>
        await PostAsyncWithResponse<EncryptedMessageModel, CreateEncryptedMessageModel>("message",
            createEncryptedMessageModel);
    
    public async Task<HttpResponseMessage> LoginAsync(LoginModel loginModel) =>
        await PostAsync("login", loginModel);

    public async Task<HttpResponseMessage> RegisterAsync(RegisterModel registerModel) =>
        await PostAsync("register", registerModel);


    private async Task<HttpResponseMessage> PostAsync<TRequest>(string requestUrl, TRequest request)
    {
        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(requestUrl, content);
        return response;
    }
    
    private async Task<TResponse> PostAsyncWithResponse<TResponse, TRequest>(string requestUrl, TRequest request)
    {
        var response = await PostAsync(requestUrl, request);
        
        var message = JsonSerializer.Deserialize<TResponse>(await response.Content.ReadAsStringAsync())!;
        return message;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}