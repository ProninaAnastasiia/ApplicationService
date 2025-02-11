using ApplicationService.Contracts;
using ApplicationService.Data.Models;

namespace ApplicationService.Services;

public class ApiGatewayService: IApiGatewayService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<ApiGatewayService> _logger;
    private readonly IConfiguration _configuration;
    
    public ApiGatewayService(IHttpClientFactory clientFactory, ILogger<ApiGatewayService> logger, IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<HttpResponseMessage> CheckAntifraudAsync(Application application)
    {
        var client = _clientFactory.CreateClient("ApiGatewayClient"); // Use the named client
        var apiGatewayUrl = _configuration.GetConnectionString("ApiGateway"); // Read from configuration (appsettings.json)

        try
        {
            var response = await client.PostAsJsonAsync($"{apiGatewayUrl}/api/validate/application", application);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Error calling ApiGateway: {ex.Message}", ex);
            // Consider implementing retry logic here.
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error calling ApiGateway: {ex.Message}", ex);
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}