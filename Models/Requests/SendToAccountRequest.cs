using Prometeus.Services.DataAnnotations;

namespace Prometeus.Services.Models.Requests
{
    /// <summary>
    /// Запрос на перевод средств между счетами внутри банка
    /// </summary>
    public class SendToAccountRequest
    {
        /// <summary>
        /// Номер счёта отправителя (20 цифр)
        /// </summary>
        [AccountNumber]
        public required string FromAccount { get; set; }

        /// <summary>
        /// Номер счёта получателя (20 цифр)
        /// </summary>
        [AccountNumber]
        public required string ToAccount { get; set; }

        /// <summary>
        /// Сумма перевода (должна быть больше 0)
        /// </summary>
        [NonZero]
        public required decimal Amount { get; set; }
    }
}
