using System.Reflection;
using System.Text.Json;
using ApplicationService.Contracts;
using ApplicationService.Data;
using ApplicationService.Data.Models;
using ApplicationService.Events;
using ApplicationService.Messages;
using ApplicationService.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IApiGatewayService, ApiGatewayService>();
builder.Services.AddSingleton<KafkaProducerService>();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Метод для получения заявки
app.MapPost("/api/application", async (Application application, ApplicationDbContext db, 
    IApiGatewayService ApiGatewayService , IPublisher publisher) =>
{
    // Логируем полученные данные в консоль
    Console.WriteLine($"Received application: {application.LoanPurpose}, {application.LoanAmount}, {application.LoanTermMonths}");
    Console.WriteLine($"User Passport: {application.User.Passport}, LoanType: {application.LoanType}, InterestRate: {application.InterestRate}");
    Console.WriteLine($"User: {application.User.FirstName} {application.User.LastName}, Email: {application.User.Email}");

    // Сохраняем заявку в базу данных
    try
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Passport.Equals(application.User.Passport));
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
            await db.SaveChangesAsync(); 
        }
        else
        {
            application.Status = "Создана";
            db.Applications.Add(application);
            await db.SaveChangesAsync(); 
            
            Console.WriteLine($"Заявка прошла проверку: {validationResult.StatusCode}");
            
            // Publish ApplicationSubmittedEvent
            var applicationSubmittedEvent = new ApplicationSubmittedEvent(
                application.User.FirstName,
                application.User.LastName,
                application.User.Age,
                application.User.Passport,
                application.User.INN,
                application.User.Gender,
                application.User.MaritalStatus,
                application.User.Education,
                application.User.EmploymentType,
                application.LoanPurpose,
                application.LoanAmount,
                application.LoanTermMonths,
                application.LoanType,
                application.InterestRate,
                application.PaymentType
            );

            await publisher.Publish(applicationSubmittedEvent); // Publish the event
            Console.WriteLine("ApplicationSubmittedEvent published.");  //Log that the message was published.
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.Problem($"Ошибка при обработке заявки: {e.Message}");
    }
    
    // Возвращаем успешный ответ
    return Results.Ok("Application processed successfully.");
});

app.MapGet("/api/test/raise-message", async (KafkaProducerService kafkaProducerService) =>
{
    var airportCreatedMessage = new ApplicationSubmittedMessage("Борис", "Тестовый", 
        23, "112233445566", "9999999999", "мужской", "эээ", 
        "высшее", "работник компании", "на айфон", 100000 , 
        3, "потребительский", 16,
        "дифф",new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString());
        
    await kafkaProducerService.ProduceAsync("ApplicationSubmitted", JsonSerializer.Serialize(airportCreatedMessage));
    return "Hello World!";
});

app.Run();
