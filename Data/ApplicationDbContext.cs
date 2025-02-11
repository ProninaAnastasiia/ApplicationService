using ApplicationService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationService.Data;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Application> Applications { get; set; }
    public DbSet<User> Users { get; set; } // Добавляем DbSet для User

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Конфигурация таблицы User
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id); // Явно указываем Id как первичный ключ
        
        // Конфигурация таблицы Application
        modelBuilder.Entity<Application>()
            .HasKey(a => a.Id); 
        
        // Настройка связи между Application и User
        modelBuilder.Entity<Application>()
            .HasOne(a => a.User)
            .WithMany(u => u.Applications)
            .IsRequired() // Указываем, что UserId обязателен (заявка не может существовать без пользователя)
            .OnDelete(DeleteBehavior.Cascade); // Настраиваем поведение при удалении пользователя (каскадное удаление заявок)
        
        base.OnModelCreating(modelBuilder);
    }
}