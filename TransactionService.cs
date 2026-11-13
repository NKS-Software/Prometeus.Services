using Prometeus.Core.Constants;
using Prometeus.Core.Data.Context;
using Prometeus.Core.Data.Entities;
using Prometeus.Core.Data.Repositories;
using Prometeus.Core.Generators;
using Prometeus.Core.Models;
using Prometeus.Services.Configs;
using Prometeus.Services.Models.DTOs;
using Prometeus.Services.Models.Requests;
using Prometeus.Services.Models.Responses;

namespace Prometeus.Services
{
    public class TransactionService(DatabaseContext databaseContext, BankDetails bankDetails)
    {
        private readonly AccountRepository accountRepository = databaseContext.Get<AccountRepository>();
        private readonly CardRepository cardRepository = databaseContext.Get<CardRepository>();
        private readonly TransactionRepository transactionRepository = databaseContext.Get<TransactionRepository>();

        /// <summary>
        /// Получает список транзакций, отсортированые по времени создания
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="accountNumber">Номер счёта, необязательно</param>
        /// <param name="count">Количество элементов на странице, по умолчанию 50, должно быть больше 0</param>
        /// <param name="page">Номер страницы начиная с 1, по умолчанию 1, должно быть больше 0 </param>
        /// <remarks>Если указан номер счёта то будут возвращены транзакции только те транзакции, где учавствует этот счёт </remarks>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> со списком <see cref="List{T}"/> данных транзакций <see cref="TransactionDTO"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> GetAllAsync(Guid clientId, string? accountNumber = null,int count = 50, int page = 1)
        {
            if (count <= 0)
                return ErrorResponse.BadRequest("Incorrect quantity");
            if (page < 1)
                return ErrorResponse.BadRequest("Incorrect page");

            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                if (!await accountRepository.BelongsToClientAsync(accountNumber, clientId))
                    return ErrorResponse.Forbidden("Account belongs to another client");
            }

            List<Transaction> list = string.IsNullOrWhiteSpace(accountNumber) ? 
                await transactionRepository.GetAllForClientAsync(clientId,count,page) : 
                await transactionRepository.GetAllForAccountAsync(accountNumber,count,page);

            return SuccessResponse<List<TransactionDTO>>.WithValue(list.ConvertAll(t => TransactionDTO.FromEntity(t)));
        }

        /// <summary>
        /// Перечисляет средства по номеру счёта
        /// </summary>
        /// <remarks><b>Данный метод используется ТОЛЬКО для переводов внутри банка</b></remarks>
        /// <param name="clientId">Идентификатор отправителя</param>
        /// <param name="request"></param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с данными транзакции <see cref="TransactionDTO"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> SendToAccountAsync(Guid clientId, SendToAccountRequest request)
        {
            var fromAccount = await accountRepository.GetByNumberAsync(request.FromAccount);
            if (fromAccount == null)
                return ErrorResponse.NotFound("Sender account not exists");
            if (fromAccount.ClientId != clientId)
                return ErrorResponse.Forbidden("Account belongs to another client");
            if (fromAccount.Balance < request.Amount)
                return ErrorResponse.BadRequest("Insufficient funds");

            var toAccount = await accountRepository.GetByNumberAsync(request.ToAccount);
            if (toAccount == null)
                return ErrorResponse.NotFound("Recipiient account not exists");

            var (_, _, senderCurrency, _, _, _) = AccountNumberGenerator.Parse(request.FromAccount);
            var (_, _, recipientCurrency, _, _, _) = AccountNumberGenerator.Parse(request.ToAccount);
            if (senderCurrency != recipientCurrency)
                return ErrorResponse.BadRequest("The recipient's currency must match the sender's currency");

            databaseContext.BeginTransaction();
            try
            {
                var t = await transactionRepository.CreateAsync();
                if(t == null)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed creating transaction");
                }
                
                var ste = await transactionRepository.CreateEntryAsync(new()
                {
                    TransactionId = t.Id,
                    AccountNumber = request.FromAccount,
                    Amount = request.Amount,
                    Type = EntryType.Source
                });
                if(ste == null)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed creating transaction");
                }
                else
                {
                    t.Entries.Add(ste);
                    var res = await accountRepository.DebitAsync(request.FromAccount, request.Amount);
                    if (!res)
                    {
                        databaseContext.Rollback();
                        return ErrorResponse.BadRequest("Failed creating transaction");
                    }
                }

                var rte = await transactionRepository.CreateEntryAsync(new()
                {
                    TransactionId = t.Id,
                    AccountNumber = request.ToAccount,
                    Amount = request.Amount,
                    Type = EntryType.Destination
                });
                if (rte == null)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed creating transaction");
                }
                else
                {
                    t.Entries.Add(rte);
                    var res = await accountRepository.CreditAsync(request.ToAccount, request.Amount);
                    if (!res)
                    {
                        databaseContext.Rollback();
                        return ErrorResponse.BadRequest("Failed creating transaction");
                    }
                }
                var tres = await transactionRepository.CompleteAsync(t.Id);
                if (!tres)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed completing transaction");
                }
                databaseContext.Commit();
                t.CompletedAt = t.CreatedAt;
                t.Status = TransactionStatus.Completed;
                return SuccessResponse<TransactionDTO>.WithValue(TransactionDTO.FromEntity(t));
            }
            catch (Exception)
            {
                databaseContext.Rollback();
                return ErrorResponse.InternalError();
            }
        }

        /// <summary>
        /// Перечисляет средства по номеру карты
        /// </summary>
        /// <remarks><b>Данный метод используется ТОЛЬКО для переводов внутри банка</b></remarks>
        /// <param name="clientId">Идентификатор отправителя</param>
        /// <param name="request"></param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с данными транзакции <see cref="TransactionDTO"/>, иначе <see cref="ErrorResponse"/></returns>

        public async Task<BaseResponse> SendToCardAsync(Guid clientId, SendToCardRequest request)
        {
            var fromAccount = await accountRepository.GetByNumberAsync(request.FromAccount);
            if (fromAccount == null)
                return ErrorResponse.NotFound("Sender account not exists");
            if (fromAccount.ClientId != clientId)
                return ErrorResponse.Forbidden("Account belongs to another client");
            if (fromAccount.Balance < request.Amount)
                return ErrorResponse.BadRequest("Insufficient funds");

            var bin = CardNumberGenerator.ExtractBin(request.ToCard);
            bool isSupported = false;
            foreach(var bins in bankDetails.BINs.Values)
            {
                if (bins.Contains(bin))
                {
                    isSupported = true;
                    break;
                }
            }
            if (!isSupported)
                return ErrorResponse.BadRequest("Card is unsupported");

            var card = await cardRepository.GetByNumberAsync(request.ToCard);
            if (card == null)
                return ErrorResponse.NotFound("Card not found");

            if (!await accountRepository.PossibleCreditAsync(card.AccountNumber))
                return ErrorResponse.BadRequest("This card cannot be credit");

            if (card.Status != CardStatus.Active)
                return ErrorResponse.BadRequest("Card is not active");

            var (_, _, senderCurrency, _, _, _) = AccountNumberGenerator.Parse(request.FromAccount);
            var (_, _, recipientCurrency, _, _, _) = AccountNumberGenerator.Parse(card.AccountNumber);
            if (senderCurrency != recipientCurrency)
                return ErrorResponse.BadRequest("The recipient's currency must match the sender's currency");

            databaseContext.BeginTransaction();
            try
            {
                var t = await transactionRepository.CreateAsync();
                if (t == null)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed creating transaction");
                }

                var ste = await transactionRepository.CreateEntryAsync(new()
                {
                    TransactionId = t.Id,
                    AccountNumber = request.FromAccount,
                    Amount = request.Amount,
                    Type = EntryType.Source
                });
                if (ste == null)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed creating transaction");
                }
                else
                {
                    t.Entries.Add(ste);
                    var res = await accountRepository.DebitAsync(request.FromAccount, request.Amount);
                    if (!res)
                    {
                        databaseContext.Rollback();
                        return ErrorResponse.BadRequest("Failed creating transaction");
                    }
                }

                var rte = await transactionRepository.CreateEntryAsync(new()
                {
                    TransactionId = t.Id,
                    AccountNumber = card.AccountNumber,
                    Amount = request.Amount,
                    Type = EntryType.Destination
                });
                if (rte == null)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed creating transaction");
                }
                else
                {
                    t.Entries.Add(rte);
                    var res = await accountRepository.CreditAsync(card.AccountNumber, request.Amount);
                    if (!res)
                    {
                        databaseContext.Rollback();
                        return ErrorResponse.BadRequest("Failed creating transaction");
                    }
                }
                t.Metadata.Add(MetadataKeys.CardNumber, request.ToCard);
                var tres = await transactionRepository.UpdateMetadataAsync(t.Id, t.Metadata);
                if (!tres)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed completing transaction");
                }

                tres = await transactionRepository.CompleteAsync(t.Id);
                if (!tres)
                {
                    databaseContext.Rollback();
                    return ErrorResponse.BadRequest("Failed completing transaction");
                }
                databaseContext.Commit();
                t.CompletedAt = t.CreatedAt;
                t.Status = TransactionStatus.Completed;
                return SuccessResponse<TransactionDTO>.WithValue(TransactionDTO.FromEntity(t));
            }
            catch (Exception)
            {
                databaseContext.Rollback();
                return ErrorResponse.InternalError();
            }
        }
    }
}
