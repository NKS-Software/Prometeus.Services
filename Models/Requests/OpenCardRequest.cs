using Prometeus.Services.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Prometeus.Services.Models.Requests
{
    /// <summary>
    /// Запрос на выпуск новой карты
    /// </summary>
    public record OpenCardRequest
    {
        /// <summary>
        /// Номер счёта, к которому привязывается карта (20 цифр)
        /// </summary>
        [AccountNumber]
        public required string AccountNumber { get; set; }

        /// <summary>
        /// Платёжная система (например, Visa, Mastercard, Mir)
        /// </summary>
        public required string PaymentSystem { get; set; }

        /// <summary>
        /// PIN-код карты (4 цифры). Опционально
        /// </summary>
        [MinLength(4)]
        [MaxLength(4)]
        public int Pin { get; set; }

        /// <summary>
        /// Признак виртуальной карты (без физического носителя)
        /// </summary>
        public bool IsVirtual { get; set; }
    }
}
