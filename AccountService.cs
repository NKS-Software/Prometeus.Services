using Prometeus.Core.Data.Context;
using Prometeus.Core.Data.Entities;
using Prometeus.Core.Data.Repositories;
using Prometeus.Core.Generators;
using Prometeus.Services.Configs;
using Prometeus.Services.Models.DTOs;
using Prometeus.Services.Models.Requests;
using Prometeus.Services.Models.Responses;
using TimeMachine.Core;

namespace Prometeus.Services
{
    public class AccountService(DatabaseContext databaseContext, BankDetails bankDetails)
    {
        private readonly AccountRepository accountRepository = databaseContext.Get<AccountRepository>();

        /// <summary>
        /// Получает информацию о счёте включая привязанные карты
        /// </summary>
        /// <param name="accountNumber">Номер счёта</param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с информацией о счёте <see cref="AccountDTO"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> GetByNumberAsync(string accountNumber)
        {
            if (!AccountNumberGenerator.Validate(accountNumber, bankDetails.BIK))
                return ErrorResponse.BadRequest("Incorrect account number");

            var cardRepository = databaseContext.Get<CardRepository>();
            var account = await accountRepository.GetByNumberAsync(accountNumber);
            if (account == null)
                return ErrorResponse.NotFound("Not found account");

            account.Cards = await cardRepository.GetAllByAccountNumberAsync(accountNumber);

            return SuccessResponse<AccountDTO>.WithValue(AccountDTO.FromEntity(account));
        }

        /// <summary>
        /// Возвращает полные реквизиты счёта клиента
        /// </summary>
        /// <param name="number">Номер счёта </param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с реквизитами счёта <see cref="AccountDetails"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> GetAccountDetails(string number, Guid clientId)
        {
            if (!AccountNumberGenerator.Validate(number, bankDetails.BIK))
                return ErrorResponse.BadRequest("Invalid account number");

            var account = await accountRepository.GetByNumberAsync(number);
            if (account == null)
                return ErrorResponse.NotFound("Account not exists");

            if (account.ClientId != clientId)
                return ErrorResponse.Forbidden("Account not belongs to client");

            var (_, _, currencyCode, _, branchCode, _) = AccountNumberGenerator.Parse(number);
            var clientRepository = databaseContext.Get<ClientRepository>();
            var client = await clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return ErrorResponse.NotFound("Client not exists");

            return SuccessResponse<AccountDetails>.WithValue(new()
            {
                Bank = bankDetails.Name,
                BIK = bankDetails.BIK,
                License = bankDetails.License,
                CorrespondentAccountNumber = AccountNumberGenerator.Generate(308, 01, currencyCode, bankDetails.BIK),
                ClientAccountNumber = number,
                ClientFullName = string.Join(" ", new[] { client.Surname, client.Name, client.Patronimyc }
                    .Where(s => !string.IsNullOrWhiteSpace(s)))
            });
        }

        /// <summary>
        /// Получает все счета клиента включая привязанные карты
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> со списком счетов <see cref="AccountDTO"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> GetAllForClientAsync(Guid clientId)
        {
            var clientRepository = databaseContext.Get<ClientRepository>();
            var cardRepository = databaseContext.Get<CardRepository>();
            if (!await clientRepository.IsExistsAsync(clientId))
                return ErrorResponse.NotFound("Client not exists");

            var accounts = await accountRepository.GetByClientIdAsync(clientId);
            foreach (var account in accounts)
            {
                account.Cards = await cardRepository.GetAllByAccountNumberAsync(account.Number);
            }
            return SuccessResponse<List<AccountDTO>>.WithValue(
                accounts.ConvertAll(a => AccountDTO.FromEntity(a))
            );
        }
        /// <summary>
        /// Открывает новый счёт
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request">Информация для открытия нового счёта</param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с информацией о новом счёте <see cref="AccountDTO"/>, иначе <see cref="ErrorResponse"/></returns>

        public async Task<BaseResponse> OpenAsync(Guid clientId, OpenAccountRequest request)
        {
            var clientRepository = databaseContext.Get<ClientRepository>();
            if (!await clientRepository.IsExistsAsync(clientId))
                return ErrorResponse.NotFound("Client not exists");

            if (!bankDetails.CurrencyCodes.TryGetValue(request.CurrencyCode, out var code) || code == 0)
                return ErrorResponse.BadRequest("Currency is not support");

            var number = AccountNumberGenerator.Generate(408, 02, code, bankDetails.BIK, request.BranchCode);
            var account = new Account
            {
                ClientId = clientId,
                Number = number,
                OpenedAt = SharedTimeProvider.Read(DateTime.UtcNow),
                Name = string.IsNullOrWhiteSpace(request.Name) ? "Payment account" : request.Name
            };
            account = await accountRepository.OpenAsync(account);
            if (account == null)
                return ErrorResponse.BadRequest("Failed open account. Try again.");
            return SuccessResponse<AccountDTO>.WithValue(AccountDTO.FromEntity(account));
        }

        /// <summary>
        /// Закрывает счёт
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="number">Номер счёта</param>
        /// <returns>В случае успеха <see cref="SuccessResponse"/>, иначе <see cref="ErrorResponse"/></returns>

        public async Task<BaseResponse> CloseAsync(Guid clientId, string number)
        {
            if (!AccountNumberGenerator.Validate(number, bankDetails.BIK))
                return ErrorResponse.BadRequest("Invalid account number");

            if (!await accountRepository.IsExistsAsync(number))
                return ErrorResponse.NotFound("Account not exists");

            if (!await accountRepository.BelongsToClientAsync(number, clientId))
                return ErrorResponse.Forbidden("Account not belongs to client");

            var res = await accountRepository.CloseAsync(number);
            return res ? SuccessResponse.WithMessage("Account success closed") :
                ErrorResponse.BadRequest("Failed closing account");
        }

        /// <summary>
        /// Обновляет счёт
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request">Обновлённая информация о счёте</param>
        /// <returns>В случае успеха <see cref="SuccessResponse"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> UpdateAsync(Guid clientId, UpdateAccountRequest request)
        {
            if (!AccountNumberGenerator.Validate(request.Number, bankDetails.BIK))
                return ErrorResponse.BadRequest("Invalid account number");

            if (!await accountRepository.BelongsToClientAsync(request.Number, clientId))
                return ErrorResponse.Forbidden("Account not belongs to client");

            if (!await accountRepository.IsExistsAsync(request.Number))
                return ErrorResponse.NotFound("Account not exists");

            var res = await accountRepository.RenameAsync(request.Number, request.Name);
            return !string.IsNullOrEmpty(res) ? SuccessResponse.WithMessage("Account success updated") :
                ErrorResponse.BadRequest("Failed updating account");
        }
    }
}
