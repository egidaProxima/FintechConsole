namespace FintechConsole.Data.Entities;
public class User
{
    public int Id { get; set; } // Первичный ключ. База сама будет увеличивать его (+1) при создании юзера
    public string Name { get; set; } = string.Empty; // Имя клиента
    public decimal Balance { get; set; } // Баланс. Помнишь? Используем строго decimal для точности!
    public decimal CashbackBalance { get; set; } = 0m; // Изначально копилка пуста
    public bool IsBlocked { get; set; } = false; // По умолчанию все новые пользователи активны


    // Навигационное свойство. Показывает Entity Framework, что у ОДНОГО юзера 
    // может быть СПИСОК из МНОГИХ транзакций (связь один-ко-многим)
    public List<Transaction> Transactions { get; set; } = new();
}
