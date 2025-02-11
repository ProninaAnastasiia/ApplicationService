using System.Text.Json;
using ApplicationService.Events;
using ApplicationService.Messages;
using ApplicationService.Services;
using MediatR;

namespace ApplicationService.Handlers;

public class ApplicationSubmittedEventHandler: INotificationHandler<ApplicationSubmittedEvent>
{
    private readonly KafkaProducerService _kafkaProducerService;

    public ApplicationSubmittedEventHandler(KafkaProducerService kafkaProducerService)
    {
        _kafkaProducerService = kafkaProducerService;
    }

    public async Task Handle(ApplicationSubmittedEvent notification, CancellationToken cancellationToken)
    {
        var airportCreatedMessage = new ApplicationSubmittedMessage(notification.FirstName, notification.LastName, 
            notification.Age, notification.Passport, notification.INN, notification.Gender, notification.MaritalStatus, 
            notification.Education, notification.EmploymentType, notification.LoanPurpose, notification.LoanAmount , 
            notification.LoanTermMonths, notification.LoanType, notification.InterestRate,
            notification.PaymentType,new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString());
        
        await _kafkaProducerService.ProduceAsync("ApplicationSubmitted", JsonSerializer.Serialize(airportCreatedMessage));
    }
}