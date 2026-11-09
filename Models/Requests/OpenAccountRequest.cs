using Prometeus.Services.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Prometeus.Services.Models.Requests
{
    /// <summary>
    /// Запрос на открытие нового счёта
    /// </summary>
    public class OpenAccountRequest
    {
        /// <summary>
        /// Название счёта (от 10 до 128 символов)
        /// </summary>
        [MaxLength(128)]
        [MinLength(10)]
        public required string Name { get; set; }

        /// <summary>
        /// Код валюты по ISO 4217 (например, RUB, USD, EUR)
        /// </summary>
        [CurrencyCode]
        public required string CurrencyCode { get; set; }

        /// <summary>
        /// Номер отделения банка (по умолчанию 0)
        /// </summary>
        public int BranchCode { get; set; } = 0;
    }
}
