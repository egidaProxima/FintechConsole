using System;
using FintechConsole.Data;
using FintechConsole.Services;

Console.WriteLine("=== Проверка системы лояльности Т-Банка ===");

using (var context = new AppDbContext())
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    ITransactionService transactionService = new TransactionService(context);

    // 1. Создаем Ивана со стартовым балансом
    await transactionService.RegisterUserAsync("Иван", 50000m);
    int ivanId = 1;

    // 2. Иван делает покупку в повышенной категории на неровную сумму
    Console.WriteLine("\n[Тест 13] Иван покупает продукты на сумму 1234.56 руб...");
    await transactionService.CreateTransactionAsync(ivanId, 1234.56m, "Супермаркеты");

    // 3. Иван делает покупку в обычной категории (где кешбэка нет)
    Console.WriteLine("\n[Тест 14] Иван покупает билеты в кино на сумму 500 руб...");
    await transactionService.CreateTransactionAsync(ivanId, 500m, "Кино");

    // 4. Проверяем балансы в БД
    var ivan = await context.Users.FindAsync(ivanId);

    Console.WriteLine($"\n=== Итоги работы системы лояльности ===");
    Console.WriteLine($"Пользователь: {ivan?.Name}");
    Console.WriteLine($"Основной баланс: {ivan?.Balance} руб.");
    Console.WriteLine($"Баланс копилки (Кешбэк): {ivan?.CashbackBalance} руб. (Ожидаем строго 12.35!)");

    Console.WriteLine("\n[Тест 15] Иван выводит накопленный кешбэк (12.35 руб) на карту...");

    // Проверяем балансы Ивана до вывода кешбэка
    var ivanBeforeWithdraw = await context.Users.FindAsync(ivanId);
    Console.WriteLine($"Основной счет до вывода: {ivanBeforeWithdraw?.Balance} руб.");
    Console.WriteLine($"Копилка до вывода: {ivanBeforeWithdraw?.CashbackBalance} руб.");

    // Вызываем твой новый метод!
    var isWithdrawSuccess = await transactionService.WithdrawCashbackAsync(ivanId);
    Console.WriteLine($"Результат вывода кешбэка: {isWithdrawSuccess}");

    // Проверяем балансы после вывода
    var ivanAfterWithdraw = await context.Users.FindAsync(ivanId);
    Console.WriteLine($"\n=== Итог вывода кешбэка ===");
    Console.WriteLine($"Основной счет после вывода: {ivanAfterWithdraw?.Balance} руб.");
    Console.WriteLine($"Копилка после вывода (должно быть 0): {ivanAfterWithdraw?.CashbackBalance} руб.");

}

Console.WriteLine("\nНажмите любую клавишу для завершения...");
Console.ReadKey();
