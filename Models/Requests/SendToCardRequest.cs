using Prometeus.Services.DataAnnotations;

namespace Prometeus.Services.Models.Requests
{
    /// <summary>
    /// Запрос на перевод средств на карту внутри банка
    /// </summary>
    public class SendToCardRequest
    {
        /// <summary>
        /// Номер счёта отправителя (20 цифр)
        /// </summary>
        [AccountNumber]
        public required string FromAccount { get; set; }

        /// <summary>
        /// Номер карты получателя (12-19 цифр)
        /// </summary>
        [CardNumber]
        public required string ToCard { get; set; }

        /// <summary>
        /// Сумма перевода (должна быть больше 0)
        /// </summary>
        [NonZero]
        public required decimal Amount { get; set; }
    }
}
