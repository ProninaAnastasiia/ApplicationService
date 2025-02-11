namespace ApplicationService.Data.Models;

public class Application
{
    public int Id { get; set; } // Уникальный идентификатор заявки
    public int UserId { get; set; } // Внешний ключ для пользователя
    public string LoanPurpose { get; set; } // Цель кредита
    public double LoanAmount { get; set; } // Сумма кредита
    public int LoanTermMonths { get; set; } // Срок кредита в месяцах
    public string LoanType { get; set; } // Тип кредита (например, ипотека, автокредит)
    public double InterestRate { get; set; } // Процентная ставка
    public string PaymentType { get; set; } // Тип платежей (аннуитетные, дифференцированные)
    public string Status { get; set; } // Статус заявки (например, "В процессе", "Одобрена", "Отказано")
    public User User { get; set; }
}