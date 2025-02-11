namespace ApplicationService.Data.Models;

public class User
{
    public int Id { get; set; } // Уникальный идентификатор пользователя
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Passport { get; set; } // Номер паспорта
    public string INN { get; set; } // ИНН
    public int Age { get; set; } // Возраст
    public string Gender { get; set; } // Пол
    public string MaritalStatus { get; set; } // Семейное положение
    public string Education { get; set; } // Образование
    public string EmploymentType { get; set; }
    
    public virtual List<Application> Applications { get; set; }
}