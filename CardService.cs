using Prometeus.Core.Data.Context;
using Prometeus.Core.Data.Entities;
using Prometeus.Core.Data.Repositories;
using Prometeus.Core.Generators;
using Prometeus.Core.Storage;
using Prometeus.Core.Utils;
using Prometeus.Services.Configs;
using Prometeus.Services.Models.DTOs;
using Prometeus.Services.Models.Requests;
using Prometeus.Services.Models.Responses;
using TimeMachine.Core;

namespace Prometeus.Services
{
    public class CardService(DatabaseContext databaseContext, BankDetails bankDetails, KeyStore keyStore)
    {
        private readonly CardRepository cardRepository = databaseContext.Get<CardRepository>();

        /// <summary>
        /// Получает базовую информацию о карте по её номеру
        /// </summary>
        /// <param name="cardNumber">Номер карты</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с информацией о карте <see cref="CardDTO"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> GetBasicInfo(string cardNumber, Guid clientId)
        {
            var res = await VerifyCardAsync(cardNumber, clientId);
            if (res is not SuccessResponse<Card> successResponse) return res;
            var card = successResponse.Value;

            return SuccessResponse<CardDTO>.WithValue(CardDTO.FromEntity(card));
        }

        /// <summary>
        /// Получает информацию о карте по её номеру, включая CVC
        /// </summary>
        /// <param name="cardNumber">Номер карты</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с информацией о карте <see cref="FullCardDto"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> GetFullInfo(string cardNumber, Guid clientId)
        {
            var res = await VerifyCardAsync(cardNumber, clientId);
            if (res is not SuccessResponse<Card> successResponse) return res;
            var card = successResponse.Value;

            var cvcKeyId = await cardRepository.GetCvcKeyIdAsync(cardNumber);
            if (!cvcKeyId.HasValue)
                return ErrorResponse.BadRequest("Failed getting full card info");

            var cvc = await keyStore.ReadAsync(cvcKeyId.Value);
            if(string.IsNullOrWhiteSpace(cvc))
                return ErrorResponse.InternalError();

            return SuccessResponse<FullCardDto>.WithValue(FullCardDto.FromEntity(card, cvc));
        }

        /// <summary>
        /// Выпускает новую карту
        /// </summary>
        /// <param name="request">Информация для выпуска карты</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> с информацией о карте <see cref="FullCardDto"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> OpenAsync(Guid clientId, OpenCardRequest request)
        {
            if (!bankDetails.BINs.TryGetValue(request.PaymentSystem, out var bins))
                return ErrorResponse.NotFound("Payment system is unsupported");
            
            var clientRepository = databaseContext.Get<ClientRepository>();
            var client = await clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return ErrorResponse.NotFound("Client not found");

            var accountRepository = databaseContext.Get<AccountRepository>();
            if (!await accountRepository.IsExistsAsync(request.AccountNumber))
                return ErrorResponse.NotFound("Account not found");

            if (!await accountRepository.BelongsToClientAsync(request.AccountNumber, clientId))
                return ErrorResponse.Forbidden("Account belongs to another client");

            var pin = request.Pin > 0 ? Random.Shared.Next(1000, 10000).ToString() : request.Pin.ToString();
            var cvc = Random.Shared.Next(100, 1000).ToString();
            var cvcKeyId = await keyStore.WriteAsync(cvc);
            var cardHolderName = request.IsVirtual ? "Virtual" : $"{client.Surname.ToUpper()} {client.Name.ToUpper()}";

            var card = new Card
            {
                AccountNumber = request.AccountNumber,
                Number = CardNumberGenerator.Generate(bins[Random.Shared.Next(0, bins.Length)]),
                ValidUntil = DateOnly.FromDateTime(SharedTimeProvider.Read(DateTime.UtcNow).AddYears(request.IsVirtual ? 3 : 5)),
                PinHash = Hasher.Hash(pin),
                CardHolderName = cardHolderName,
                CvcKeyId = cvcKeyId,
            };
            card = await cardRepository.OpenAsync(card);
            if (card == null)
                return ErrorResponse.BadRequest("Failed openning card");

            return SuccessResponse<FullCardDto>.WithValue(FullCardDto.FromEntity(card, cvc, pin));
        }

        /// <summary>
        /// Изменяет пин код карты
        /// </summary>
        /// <param name="request">Запрос для изменения пин кода</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> ChangePinAsync(Guid clientId,ChangePinRequest request)
        {
            var res = await VerifyCardAsync(request.CardNumber, clientId);
            if (res is not SuccessResponse<Card>) return res;
            var b = await cardRepository.ChangePinHashAsync(request.CardNumber, Hasher.Hash(request.NewPin.ToString()));
            return b ? SuccessResponse.WithMessage($"Pin for card changed") :
                ErrorResponse.BadRequest("Failed for changing pin for card");
        }

        /// <summary>
        /// Получает список карт привязанные к указанному счёту
        /// </summary>
        /// <param name="accountNumber">Номер счёта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse{T}"/> со списком карт <see cref="List{T}"/> <see cref="CardDTO"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> GetAllForAccountAsync(Guid clientId, string accountNumber)
        {
            var accountRepository = databaseContext.Get<AccountRepository>();
            if (!await accountRepository.BelongsToClientAsync(accountNumber, clientId))
                return ErrorResponse.Forbidden("Account belongs to another client");

            var cards = await cardRepository.GetAllByAccountNumberAsync(accountNumber);
            return SuccessResponse<List<CardDTO>>.WithValue(cards != null ? cards.ConvertAll(c => CardDTO.FromEntity(c)) : []);
        }

        /// <summary>
        /// Замораживает карту
        /// </summary>
        /// <param name="cardNumber">Номер карты</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> FreezeAsync(Guid clientId, string cardNumber)
        {
            var res = await VerifyCardAsync(cardNumber, clientId);
            if (res is not SuccessResponse<Card>) return res;
            var b = await cardRepository.FreezeAsync(cardNumber);
            return b ? SuccessResponse.WithMessage($"Card freezed") :
                ErrorResponse.BadRequest("Failed freezing card");
        }

        /// <summary>
        /// Размораживает карту
        /// </summary>
        /// <param name="cardNumber">Номер карты</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> UnfreezeAsync(Guid clientId, string cardNumber)
        {
            var card = await cardRepository.GetByNumberAsync(cardNumber);
            if (card == null) return ErrorResponse.NotFound("Card not exists");

            // Проверка на принадлежность клиенту
            var accountRepository = databaseContext.Get<AccountRepository>();
            if (!await accountRepository.BelongsToClientAsync(card.AccountNumber, clientId))
                return ErrorResponse.Forbidden("Card belongs to another client");

            var b = await cardRepository.UnfreezeAsync(cardNumber);
            return b ? SuccessResponse.WithMessage($"Card unfreezed") :
                ErrorResponse.BadRequest("Failed unfreezing card");
        }

        /// <summary>
        /// Перманентно блокирует карту
        /// </summary>
        /// <param name="cardNumber">Номер карты</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>В случае успеха <see cref="SuccessResponse"/>, иначе <see cref="ErrorResponse"/></returns>
        public async Task<BaseResponse> LockAsync(Guid clientId, string cardNumber)
        {
            var res = await VerifyCardAsync(cardNumber, clientId);
            if (res is not SuccessResponse<Card>) return res;
            var b = await cardRepository.LockAsync(cardNumber);
            return b ? SuccessResponse.WithMessage($"Card locked") :
                ErrorResponse.BadRequest("Failed locking card");
        }

        private async Task<BaseResponse> VerifyCardAsync(string cardNumber, Guid clientId)
        {
            var isOurBank = bankDetails.BINs.Values
                .SelectMany(bins => bins)
                .Any(bin => cardNumber.StartsWith(bin));
            if (!isOurBank)
                return ErrorResponse.BadRequest("This card was issued by another issuer.");

            var card = await cardRepository.GetByNumberAsync(cardNumber);
            if (card == null)
                return ErrorResponse.NotFound("Card not exists");
            if (card.Status != CardStatus.Active)
                return ErrorResponse.BadRequest("Card is not active");

            var accountRepository = databaseContext.Get<AccountRepository>();
            if (!await accountRepository.BelongsToClientAsync(card.AccountNumber, clientId))
                return ErrorResponse.Forbidden("Сard belongs to another client");
            return SuccessResponse<Card>.WithValue(card);
        }
    }
}
