using Prometeus.Core.Data.Entities;

namespace Prometeus.Services.Models.DTOs;

/// <summary>
/// DTO для отображения информации о счёте
/// </summary>
public class AccountDTO
{
    /// <summary>
    /// Номер счёта
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// Название счёта
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Текущий баланс
    /// </summary>
    public required decimal Balance { get; init; }

    /// <summary>
    /// Список привязанных карт
    /// </summary>
    public List<CardDTO> Cards { get; init; } = [];

    /// <summary>
    /// Создаёт DTO из сущности счёта
    /// </summary>
    public static AccountDTO FromEntity(Account account) => new()
    {
        Number = account.Number,
        Name = account.Name,
        Balance = account.Balance,
        Cards = account.Cards.ConvertAll(c => CardDTO.FromEntity(c)),
    };
}
