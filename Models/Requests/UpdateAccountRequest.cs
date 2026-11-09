using Prometeus.Services.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Prometeus.Services.Models.Requests
{
    /// <summary>
    /// Запрос на обновление названия счёта
    /// </summary>
    public class UpdateAccountRequest
    {
        /// <summary>
        /// Номер счёта (20 цифр)
        /// </summary>
        [AccountNumber]
        public required string Number { get; set; }

        /// <summary>
        /// Новое название счёта (от 10 до 128 символов)
        /// </summary>
        [MaxLength(128)]
        [MinLength(10)]
        public required string Name { get; set; }
    }
}
