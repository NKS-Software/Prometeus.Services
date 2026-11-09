using Prometeus.Core.Data.Entities;
namespace Prometeus.Services.Models.DTOs
{
    /// <summary>
    /// DTO для отображения полной информации о карте (включая CVC и PIN)
    /// </summary>
    public record FullCardDto:CardDTO
    {
        /// <summary>
        /// CVC-код карты
        /// </summary>
        public string Cvc { get; set; } = "";

        /// <summary>
        /// PIN-код карты
        /// </summary>
        public string Pin { get; set; } = "";

        /// <summary>
        /// Создаёт DTO из сущности карты с дополнительными данными
        /// </summary>
        /// <param name="card">Сущность карты</param>
        /// <param name="cvc">CVC-код (опционально)</param>
        /// <param name="pin">PIN-код (опционально)</param>
        public static FullCardDto FromEntity(Card card, string cvc = "", string pin = "") => new()
        {
            Number = card.Number,
            CardHolderName = card.CardHolderName,
            ValidUntil = card.ValidUntil,
            Status = card.Status,
            Cvc = cvc,
            Pin = pin
        };
    }
}
