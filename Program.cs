using ApplicationService.Data;
using ApplicationService.Data.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient(); // Для отправки HTTP запросов

var connectionString = builder.Configuration.GetConnectionString("Postgres"); // Настройка PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IApiGatewayService, ApiGatewayService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Метод для получения заявки
app.MapPost("/api/application", async (Application application, ApplicationDbContext db, IApiGatewayService ApiGatewayService) =>
{
    // Логируем полученные данные в консоль
    Console.WriteLine($"Received application: {application.LoanPurpose}, {application.LoanAmount}, {application.LoanTermMonths}");
    Console.WriteLine($"User Passport: {application.User.Passport}, LoanType: {application.LoanType}, InterestRate: {application.InterestRate}");
    Console.WriteLine($"User: {application.User.FirstName} {application.User.LastName}, Email: {application.User.Email}");

    // Сохраняем заявку в базу данных
    try
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Passport == application.User.Passport);
        if (existingUser == null)
        {
            db.Users.Add(application.User);
            await db.SaveChangesAsync(); // Сохраняем пользователя, чтобы получить Id
        }

        // После сохранения заявки отправляем запрос во внешнюю систему (также через ApiGateway) на валидацию пользователя
        var validationResult = await ApiGatewayService.CheckAntifraudAsync(application);
        if (!validationResult.IsSuccessStatusCode)
        {
            Console.WriteLine($"Заявка не прошла анти-фрод проверку: {validationResult.StatusCode}");
            application.Status = "Отклонена";
        }
        else
        {
            application.Status = "Создана";
            db.Applications.Add(application);
            Console.WriteLine($"Заявка прошла проверку: {validationResult.StatusCode}");
        }
        await db.SaveChangesAsync(); 
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.Problem($"Ошибка при обработке заявки: {e.Message}");
    }
    
    // Возвращаем успешный ответ
    return Results.Ok("Application processed successfully.");
});

app.Run();

public interface IApiGatewayService
{
    Task<HttpResponseMessage> CheckAntifraudAsync(Application application);
}

public class ApiGatewayService : IApiGatewayService
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




