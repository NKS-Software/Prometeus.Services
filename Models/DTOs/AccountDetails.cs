using System;
using System.Collections.Generic;
using System.Text;

namespace Prometeus.Services.Models.DTOs
{
    /// <summary>
    /// Класс для хранения полных реквизитов счёта
    /// </summary>
    public class AccountDetails
    {
        /// <summary>
        /// Наименование банка
        /// </summary>
        public required string Bank { get; set; }

        /// <summary>
        /// Банковский идентификационный код
        /// </summary>
        public required string BIK { get; set; }

        /// <summary>
        /// Номер лицензии
        /// </summary>
        public required string License { get; set; }

        /// <summary>
        /// Корреспондентский счёт банка
        /// </summary>
        public required string CorrespondentAccountNumber { get; set; }

        /// <summary>
        /// Полное имя клиента
        /// </summary>
        public required string ClientFullName { get; set; }

        /// <summary>
        /// Номер счёта клиента
        /// </summary>
        public required string ClientAccountNumber { get; set; }
    }
}
