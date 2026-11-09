using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Prometeus.Services.DataAnnotations
{
    /// <summary>
    /// Атрибут валидации номера карты
    /// </summary>
    /// <remarks>
    /// Проверяет только формат: от 12 до 19 цифр.
    /// Контрольный ключ (Luhn) проверяется в сервисе через CardNumberGenerator.Validate.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public partial class CardNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            if (value is string strvalue)
            {
                var regex = CardNumberRegex();
                return regex.IsMatch(strvalue);
            }
            else
                return false;
        }

        [GeneratedRegex(@"^[0-9]{12-19}$")]
        public static partial Regex CardNumberRegex();
    }
}
