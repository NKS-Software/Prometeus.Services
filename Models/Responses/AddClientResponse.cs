using Prometeus.Core.Models;
using Prometeus.Services.Models.DTOs;

namespace Prometeus.Services.Models.Responses;

/// <summary>
/// Ответ на успешное добавление клиента
/// </summary>
public record AddClientResponse : SuccessResponse
{
    /// <summary>
    /// Идентификатор добавленного клиента
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// DTO с данными добавленного клиента
    /// </summary>
    public required ClientDTO Client { get; init; }
}
