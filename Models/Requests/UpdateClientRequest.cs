using Prometeus.Services.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Prometeus.Services.Models.Requests;

/// <summary>
/// Запрос на обновление данных клиента (частичное обновление)
/// </summary>
public record UpdateClientRequest
{
    /// <summary>
    /// Новое имя клиента (опционально)
    /// </summary>
    [Name]
    [MinLength(2)]
    [MaxLength(128)]
    public string? Name { get; init; }

    /// <summary>
    /// Новая фамилия клиента (опционально)
    /// </summary>
    [Surname]
    [MinLength(2)]
    [MaxLength(128)]
    public string? Surname { get; init; }

    /// <summary>
    /// Новое отчество клиента (опционально)
    /// </summary>
    [Patronimyc]
    [MinLength(2)]
    [MaxLength(128)]
    public string? Patronimyc { get; init; }

    /// <summary>
    /// Новая дата рождения (опционально, клиент должен быть старше 18 лет)
    /// </summary>
    [MinAge(18)]
    public DateOnly? BirthDate { get; init; }
}
