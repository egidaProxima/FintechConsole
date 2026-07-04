using FintechConsole.Data;
using FintechConsole.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace FintechConsole.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _context;

    // Внедряем контекст нашей базы данных через конструктор
    public TransactionService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<User?> FindUser(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    // МЕТОД 1: Регистрация пользователя (Пишет ментор)
    public async Task<bool> RegisterUserAsync(string name, decimal initialBalance)
    {
        // Твоя строгая логика Fail-Fast в действии!
        if (string.IsNullOrWhiteSpace(name)) return false; // Имя не может быть пустым или из пробелов
        if (initialBalance < 0) return false;              // Стартовый баланс не может быть отрицательным

        // Создаем объект сущности
        var newUser = new User
        {
            Name = name,
            Balance = initialBalance
        };

        // Добавляем в таблицу Users
        await _context.Users.AddAsync(newUser);

        // Сохраняем изменения в файл fintech.db
        await _context.SaveChangesAsync();
        return true;
    }

    // МЕТОД 2: Создание транзакции (Твое задание!)
    public async Task<bool> CreateTransactionAsync(int userId, decimal amount, string category)
    {
        if (amount <= 0 || string.IsNullOrWhiteSpace(category)) return false;

        var user = await FindUser(userId);
        if (user == null || user.Balance < amount || user.IsBlocked) return false;

        user.Balance -= amount;

        // НОВАЯ ФИЧА: Считаем кешбэк
        if (category == "Супермаркеты" || category == "Кафе")
        {
            decimal rawCashback = amount * 0.01m; // 1% от суммы покупки

            // Применяем банковское округление до 2 знаков после запятой (до копеек)
            decimal roundedCashback = Math.Round(rawCashback, 2);

            // Начисляем в копилку
            user.CashbackBalance += roundedCashback;
            Console.WriteLine($"[ЛОЯЛЬНОСТЬ] Начислен кешбэк за категорию '{category}': +{roundedCashback} руб. в копилку!");
        }

        var transaction = new Transaction
        {
            Amount = amount,
            Category = category,
            Timestamp = DateTime.UtcNow,
            UserId = userId
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> TransferMoneyAsync(int senderId, int receiverId, decimal amount)
    {
        if (senderId == receiverId || amount <= 0) return false;

        var sender = await FindUser(senderId);
        if (sender == null || sender.Balance < amount) return false;

        if (sender.IsBlocked)
        {
            Console.WriteLine($"[БЕЗОПАСНОСТЬ] Отказано в переводе: Аккаунт ID {senderId} ЗАБЛОКИРОВАН.");
            return false;
        }

        var receiver = await FindUser(receiverId);
        if (receiver == null) return false;

        using var dbTransaction = await _context.Database.BeginTransactionAsync();

        try
        {
            sender.Balance -= amount;
            receiver.Balance += amount;

            var transaction = new Transaction
            {
                UserId = senderId,
                Amount = amount,
                Category = $"перевод пользователю {receiverId}",
                Timestamp = DateTime.UtcNow,
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            return false;
        }
    }

    public async Task<List<Transaction>> GetHistoryAsync(int userId, int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            throw new ArgumentException("Некорректные параметры пагинации. pageNumber >= 1, pageSize от 1 до 100.");
        }

        var user = await FindUser(userId);
        if (user == null) throw new KeyNotFoundException($"Пользователь с ID {userId} не найден в базе данных.");

        List<Transaction> transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize).ToListAsync();

        return transactions;
    }
    public async Task<decimal> GetTotalSpentByCategoryAsync(int userId, string category)
    {
        var user = await FindUser(userId);
        if (user == null) throw new KeyNotFoundException($"Пользователь с ID {userId} не найден в базе данных.");

        var amountByCategory = await _context.Transactions
            .Where(t => t.UserId == userId && t.Category == category)
            .SumAsync(t => t.Amount);

        return amountByCategory;
    }

    public async Task<bool> IsSuspiciousActivityAsync(int userId)
    {
        var user = await FindUser(userId);
        if(user == null) throw new KeyNotFoundException($"Пользователь с ID {userId} не найден в базе данных.");

        DateTime time = DateTime.UtcNow.AddMinutes(-5);

        var transactionsCount = await _context.Transactions
            .Where(t => t.UserId == userId && t.Timestamp >= time).CountAsync();

        return transactionsCount > 3;
    }
}
