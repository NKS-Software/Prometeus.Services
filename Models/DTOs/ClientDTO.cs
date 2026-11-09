using Prometeus.Core.Data.Entities;

namespace Prometeus.Services.Models.DTOs;

/// <summary>
/// DTO для отображения информации о клиенте
/// </summary>
public record ClientDTO
{
    /// <summary>
    /// Имя клиента
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Фамилия клиента
    /// </summary>
    public required string Surname { get; init; }

    /// <summary>
    /// Отчество клиента (опционально)
    /// </summary>
    public string? Patronimyc { get; init; }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public required DateOnly BirthDate { get; init; }

    /// <summary>
    /// Email клиента
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Телефон клиента
    /// </summary>
    public required string Phone { get; init; }

    /// <summary>
    /// Список счетов клиента
    /// </summary>
    public List<AccountDTO> Accounts { get; init; } = [];

    /// <summary>
    /// Создаёт DTO из сущности клиента
    /// </summary>
    public static ClientDTO FromEntity(Client client) => new()
    {
        Name = client.Name,
        Surname = client.Surname,
        Patronimyc = client.Patronimyc,
        BirthDate = client.BirthDate,
        Email = client.Email,
        Phone = client.Phone,
        Accounts = client.Accounts.ConvertAll(a => AccountDTO.FromEntity(a))
    };
}
