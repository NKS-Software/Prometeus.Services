using Prometeus.Core.Data.Entities;

namespace Prometeus.Services.Models.DTOs;

/// <summary>
/// DTO для отображения базовой информации о карте
/// </summary>
public record CardDTO
{
    /// <summary>
    /// Номер карты
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// Имя держателя карты
    /// </summary>
    public required string CardHolderName { get; init; }

    /// <summary>
    /// Срок действия карты
    /// </summary>
    public required DateOnly ValidUntil { get; init; }

    /// <summary>
    /// Текущий статус карты
    /// </summary>
    public required CardStatus Status { get; init; }

    /// <summary>
    /// Создаёт DTO из сущности карты
    /// </summary>
    public static CardDTO FromEntity(Card card) => new() {
        Number = card.Number,
        CardHolderName = card.CardHolderName,
        ValidUntil = card.ValidUntil,
        Status = card.Status
    };
}
