using Prometeus.Core.Data.Entities;

namespace Prometeus.Services.Models.DTOs
{
    /// <summary>
    /// DTO для отображения информации о транзакции
    /// </summary>
    public class TransactionDTO
    {
        /// <summary>
        /// Уникальный идентификатор транзакции
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Дата и время создания транзакции
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время завершения транзакции (если завершена)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Текущий статус транзакции
        /// </summary>
        public required TransactionStatus Status { get; set; }

        /// <summary>
        /// Причина отклонения (если транзакция отклонена)
        /// </summary>
        public string? RejectReason { get; set; }

        /// <summary>
        /// Номер счёта отправителя
        /// </summary>
        public required string FromAccount { get; set; }

        /// <summary>
        /// Номер счёта получателя (только клиентский счёт, начинающийся с 4)
        /// </summary>
        public required string ToAccount { get; set; }

        /// <summary>
        /// Сумма комиссии
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Итоговая сумма перевода (TotalAmount - Fee)
        /// </summary>
        public decimal Amount => TotalAmount - Fee;

        /// <summary>
        /// Общая сумма списания (включая комиссию)
        /// </summary>
        public required decimal TotalAmount { get; set; }

        /// <summary>
        /// Дополнительные метаданные транзакции
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];

        /// <summary>
        /// Создаёт DTO из сущности транзакции
        /// </summary>
        /// <param name="transaction">Сущность транзакции</param>
        /// <returns>DTO транзакции</returns>
        /// <exception cref="Exception">Если отсутствует проводка Source или Destination (клиентский счёт)</exception>
        public static TransactionDTO FromEntity(Transaction transaction) => new()
        {
            Id = transaction.Id,
            CreatedAt = transaction.CreatedAt,
            CompletedAt = transaction.CompletedAt,
            Status = transaction.Status,
            RejectReason = transaction.RejectReason,
            FromAccount = transaction.Entries.FirstOrDefault(e => e.Type == EntryType.Source)?.AccountNumber ?? throw new Exception("Source entry required"),
            ToAccount = transaction.Entries.LastOrDefault(e => e.Type == EntryType.Destination && e.AccountNumber.StartsWith('4'))?.AccountNumber ?? throw new Exception("Destination entry required"),
            TotalAmount = transaction.Entries.Where(e => e.Type == EntryType.Source).Sum(e => e.Amount),
            Fee = CalculateFeeAmount(transaction),
            Metadata = transaction.Metadata,
        };

        private static decimal CalculateFeeAmount(Transaction transaction)
        {
            var sa = transaction.Entries.FirstOrDefault(e => e.Type == EntryType.Source)?.Amount;
            var da = transaction.Entries.LastOrDefault(e => e.Type == EntryType.Destination && e.AccountNumber.StartsWith('4'))?.Amount;
            return sa.HasValue && da.HasValue ? sa.Value - da.Value : 0;
        }
    }
}
