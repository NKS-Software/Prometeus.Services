using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Prometeus.Services.DataAnnotations
{
    /// <summary>
    /// Атрибут валидации кода валюты (ISO 4217)
    /// </summary>
    /// <remarks>
    /// Формат: 3 заглавные латинские буквы. Например: USD, EUR, RUB
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public partial class CurrencyCodeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            if (value is string strvalue)
            {
                var regex = CurrencyCodeRegex();
                return regex.IsMatch(strvalue);
            }
            else
                return false;
        }

        [GeneratedRegex(@"^[A-Z]{3}$")]
        public static partial Regex CurrencyCodeRegex();
    }
}
