using Prometeus.Core.Data.Entities;
using Prometeus.Services.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Prometeus.Services.Models.Requests;

/// <summary>
/// Запрос на добавление нового клиента
/// </summary>
public record AddClientRequest
{
    /// <summary>
    /// Имя клиента (кириллица или латиница, с заглавной буквы)
    /// </summary>
    [Name]
    [MinLength(2)]
    [MaxLength(128)]
    public required string Name { get; init; }

    /// <summary>
    /// Фамилия клиента (кириллица или латиница, с заглавной буквы)
    /// </summary>
    [Surname]
    [MinLength(2)]
    [MaxLength(128)]
    public required string Surname { get; init; }

    /// <summary>
    /// Отчество клиента (опционально)
    /// </summary>
    [Patronimyc]
    [MinLength(2)]
    [MaxLength(128)]
    public string? Patronimyc { get; init; }

    /// <summary>
    /// Дата рождения (клиент должен быть старше 18 лет)
    /// </summary>
    [MinAge(18)]
    public required DateOnly BirthDate { get; init; }

    /// <summary>
    /// Адрес электронной почты (уникальный)
    /// </summary>
    [EmailAddress]
    [MinLength(10)]
    [MaxLength(256)]
    public required string Email { get; init; }

    /// <summary>
    /// Номер телефона (уникальный, формат: +7XXXXXXXXXX)
    /// </summary>
    [PhoneNumber]
    [MinLength(10)]
    [MaxLength(16)]
    public required string Phone { get; init; }

    /// <summary>
    /// Преобразует запрос в сущность <see cref="Client"/>
    /// </summary>
    /// <returns>Новый экземпляр клиента с сгенерированным GUID v7</returns>
    public Client ToEntity() => new()
    {
        Id = Guid.CreateVersion7(),
        Name = Name,
        Surname = Surname,
        Patronimyc = Patronimyc,
        BirthDate = BirthDate,
        Email = Email,
        Phone = Phone,
    };
}
