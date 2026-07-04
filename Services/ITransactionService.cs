namespace FintechConsole.Services;

using FintechConsole.Data.Entities;
public interface ITransactionService
{
    public Task<bool> RegisterUserAsync(string name, decimal initialBalance);
    public Task<bool> CreateTransactionAsync(int userId, decimal amount, string category);

    public Task<bool> TransferMoneyAsync(int senderId, int receiverId, decimal amount);

    public Task<List<Transaction>> GetHistoryAsync(int userId, int pageNumber, int pageSize);
    public Task<decimal> GetTotalSpentByCategoryAsync(int userId, string category);

    public Task<bool> IsSuspiciousActivityAsync(int userId);

}