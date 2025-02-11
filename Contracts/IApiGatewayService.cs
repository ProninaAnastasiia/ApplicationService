using ApplicationService.Data.Models;

namespace ApplicationService.Contracts;

public interface IApiGatewayService
{
    Task<HttpResponseMessage> CheckAntifraudAsync(Application application);
}