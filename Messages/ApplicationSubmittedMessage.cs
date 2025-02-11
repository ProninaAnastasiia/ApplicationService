namespace ApplicationService.Messages;

public record ApplicationSubmittedMessage(
    string FirstName, string LastName, int Age, string Passport, string INN, 
    string Gender, string MaritalStatus, string Education, string EmploymentType, 
    string LoanPurpose, double LoanAmount, int LoanTermMonths, string LoanType,
    double InterestRate, string PaymentType, string timestamp);