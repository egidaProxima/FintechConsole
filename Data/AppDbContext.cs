using FintechConsole.Data.Entities;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FintechConsole.Data;

public class AppDbContext : DbContext
{
    // Эти свойства (DbSet) говорят Entity Framework, какие таблицы должны быть в базе
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    // Этот метод настраивает подключение к самой базе данных
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Мы говорим: используй SQLite. База данных создастся автоматически 
        // в папке нашего приложения в файле с именем "fintech.db"
        optionsBuilder.UseSqlite("Data Source=fintech.db");
    }

    // Здесь мы задаем жесткие правила для колонок (валидацию на уровне БД)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Указываем, что имя пользователя является обязательным (NOT NULL)
        modelBuilder.Entity<User>()
            .Property(u => u.Name)
            .IsRequired();

        // Указываем категорию транзакции как обязательное поле
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Category)
            .IsRequired();
    }
}
