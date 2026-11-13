using Prometeus.Core.Data.Context;
using Prometeus.Core.Data.Repositories;
using Prometeus.Core.Models;
using Prometeus.Services.Models.DTOs;
using Prometeus.Services.Models.Requests;
using Prometeus.Services.Models.Responses;

namespace Prometeus.Services;

public class ClientService(DatabaseContext database)
{
    private readonly ClientRepository clientRepository = database.Get<ClientRepository>();
    private readonly AccountRepository accountRepository = database.Get<AccountRepository>();
    private readonly CardRepository cardRepository = database.Get<CardRepository>();

    /// <summary>
    /// Получает информацию о клиенте по идентификатору
    /// </summary>
    /// <param name="clientId">Идентификатор клиента</param>
    /// <returns>В случае успеха возвращает <see cref="SuccessResponse{T}"/> с данными клиента <see cref="ClientDTO"/>, иначе <see cref="ErrorResponse"/></returns>
    public async Task<BaseResponse> GetByIdAsync(Guid clientId)
    {
        var client = await clientRepository.GetByIdAsync(clientId);
        if (client == null)
            return ErrorResponse.NotFound("Client not found");
        client.Accounts = await accountRepository.GetByClientIdAsync(clientId);
        foreach (var account in client.Accounts)
        {
            account.Cards = await cardRepository.GetAllByAccountNumberAsync(account.Number);
        }
        return SuccessResponse<ClientDTO>.WithValue(ClientDTO.FromEntity(client));
    }

    /// <summary>
    /// Добавляет нового клиента
    /// </summary>
    /// <param name="request">Данные нового клиента</param>
    /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с информацией о новом клиенте <see cref="AddClientResponse"/>, иначе <see cref="ErrorResponse"/></returns>
    public async Task<BaseResponse> AddAsync(AddClientRequest request)
    {
        if (await clientRepository.EmailAlreadyUsedAsync(request.Email))
            return ErrorResponse.BadRequest("Email already used");
        if (await clientRepository.PhoneAlreadyUsedAsync(request.Phone))
            return ErrorResponse.BadRequest("Phone already used");

        var client = request.ToEntity();
        client = await clientRepository.AddAsync(client);
        if (client == null)
            return ErrorResponse.BadRequest("Failed adding client");

        var res = new AddClientResponse
        {
            Message = "Client success added",
            Id = client.Id,
            Client = ClientDTO.FromEntity(client)
        };
        return SuccessResponse<AddClientResponse>.WithValue(res);
    }

    /// <summary>
    /// Обновляет информацию о клиенте по идентификатору
    /// </summary>
    /// <param name="clientId">Идентификатор клиента</param>
    /// <param name="request">Новая информация о клиенте</param>
    /// <remarks>Через этот метод нельзя изменить номер телефона или адрес электронной почты</remarks>
    /// <returns>В случае успеха возвращает <see cref="SuccessResponse{T}"/> с данными клиента <see cref="ClientDTO"/>, иначе <see cref="ErrorResponse"/></returns>
    public async Task<BaseResponse> UpdateAsync(Guid clientId, UpdateClientRequest request)
    {
        if (!await clientRepository.IsExistsAsync(clientId))
            return ErrorResponse.NotFound("Client not found");

        var client = await clientRepository.UpdateAsync(clientId, request.Name, request.Surname, request.Patronimyc, request.BirthDate);
        if (client == null)
            return ErrorResponse.BadRequest("Failed updating client");
        
        return SuccessResponse<ClientDTO>.WithValue(ClientDTO.FromEntity(client));
    }

    /// <summary>
    /// Удаляет информацию о клиенте
    /// </summary>
    /// <param name="clientId">Идентификатор клиента</param>
    /// <remarks>Для удаления, необходимо чтобы у клиента не было активных счетов</remarks>
    /// <returns>В случае успеха <see cref="SuccessResponse"/>, иначе <see cref="ErrorResponse"/></returns>
    public async Task<BaseResponse> DeleteAsync(Guid clientId)
    {
        if (!await clientRepository.IsExistsAsync(clientId))
            return ErrorResponse.NotFound("Client not found");

        var accountRepository = database.Get<AccountRepository>();
        if (await accountRepository.HasActiveAccountsAsync(clientId))
            return ErrorResponse.BadRequest("A client cannot be deleted while there are active accounts");

        if (await clientRepository.DeleteAsync(clientId))
            return SuccessResponse.WithMessage("Client success deleted");
        else
            return ErrorResponse.BadRequest("Failed deleting client");
    }

    /// <summary>
    /// Обновляет адрес электронной почты клиента
    /// </summary>
    /// <param name="clientId">Идентификатор клиента</param>
    /// <param name="email">Новый адресс электронной почты</param>
    /// <returns>В случае успеха возвращает <see cref="SuccessResponse{T}"/> с данными клиента <see cref="ClientDTO"/>, иначе <see cref="ErrorResponse"/></returns>
    public async Task<BaseResponse> ChangeEmailAsync(Guid clientId, string email)
    {
        if (!await clientRepository.IsExistsAsync(clientId))
            return ErrorResponse.NotFound("Client not found");

        if(await clientRepository.EmailAlreadyUsedAsync(email))
            return ErrorResponse.BadRequest($"Email already used");

        var res = await clientRepository.ChangeEmailAsync(clientId, email);
        if (res != null)
            return SuccessResponse<ClientDTO>.WithValue(ClientDTO.FromEntity(res));
        else
            return ErrorResponse.BadRequest("Failed change email");
    }

    /// <summary>
    /// Обновляет номер телефона клиента
    /// </summary>
    /// <param name="clientId">Идентификатор клиента</param>
    /// <param name="phone">Новый номер телефона</param>
    /// <returns>В случае успеха возвращает <see cref="SuccessResponse{T}"/> с данными клиента <see cref="ClientDTO"/>, иначе <see cref="ErrorResponse"/></returns>

    public async Task<BaseResponse> ChangePhoneAsync(Guid clientId, string phone)
    {
        if (!await clientRepository.IsExistsAsync(clientId))
            return ErrorResponse.NotFound("Client not found");

        if (await clientRepository.PhoneAlreadyUsedAsync(phone))
            return ErrorResponse.BadRequest($"Phone already used");

        var res = await clientRepository.ChangePhoneAsync(clientId, phone);
        if (res != null)
            return SuccessResponse<ClientDTO>.WithValue(ClientDTO.FromEntity(res));
        else
            return ErrorResponse.BadRequest("Failed change email");
    }
}