namespace FintechConsole.Data.Entities;

// Это класс-модель для таблицы транзакций (истории платежей)
public class Transaction
{
    public int Id { get; set; } // Первичный ключ транзакции
    public decimal Amount { get; set; } // Сумма перевода
    public string Category { get; set; } = string.Empty; // Категория (например, "Супермаркеты")
    public DateTime Timestamp { get; set; } // Точное время, когда произошел платеж

    // Внешний ключ (Foreign Key). Он связывает эту строку транзакции с конкретным Id юзера
    public int UserId { get; set; }

    // Навигационное свойство. Позволяет в коде легко написать `transaction.User.Name`, 
    // чтобы узнать, кто совершил этот платеж
    public User User { get; set; } = null!;
}
