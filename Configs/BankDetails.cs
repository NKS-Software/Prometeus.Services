namespace Prometeus.Services.Configs
{
    /// <summary>
    /// Класс для хранения конфигурации банка
    /// </summary>
    public class BankDetails
    {
        /// <summary>
        /// Банковский идентификационный код
        /// </summary>
        public required string BIK { get; set; }

        /// <summary>
        /// Наименование банка
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Номер лицензии
        /// </summary>
        public required string License { get; set; }

        /// <summary>
        /// Набор банковских идентификационных кодов для карт
        /// </summary>
        public required Dictionary<string, string[]> BINs { get; set; }

        /// <summary>
        /// Набор кодов валюты по ОКВ
        /// </summary>
        public required Dictionary<string, int> CurrencyCodes { get; set; }
    }
}
