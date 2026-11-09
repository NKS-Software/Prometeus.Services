using Prometeus.Services.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Prometeus.Services.Models.Requests
{
    /// <summary>
    /// Запрос на изменение PIN-кода карты
    /// </summary>
    public record ChangePinRequest
    {
        /// <summary>
        /// Номер карты (12-19 цифр)
        /// </summary>
        [CardNumber]
        public required string CardNumber { get; set; }

        /// <summary>
        /// Новый PIN-код (ровно 4 цифры)
        /// </summary>
        [MinLength(4)]
        [MaxLength(4)]
        public required int NewPin { get; set; }
    }
}
