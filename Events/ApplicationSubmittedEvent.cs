using ApplicationService.Contracts;

namespace ApplicationService.Events;

public record ApplicationSubmittedEvent(
    string FirstName, string LastName, int Age, string Passport, string INN, 
    string Gender, string MaritalStatus, string Education, string EmploymentType,
    string LoanPurpose, double LoanAmount, int LoanTermMonths, string LoanType,
    double InterestRate, string PaymentType) : IEvent;