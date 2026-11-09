using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Prometeus.Services.DataAnnotations
{
    /// <summary>
    /// Атрибут валидации номера счёта (20 цифр)
    /// </summary>
    /// <remarks>
    /// Проверяет только формат: ровно 20 цифр.
    /// Контрольный ключ (MOD 10) проверяется в сервисе через AccountNumberGenerator.Validate.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public partial class AccountNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            if (value is string strvalue)
            {
                var regex = AccountNumberRegex();
                return regex.IsMatch(strvalue);
            }
            else
                return false;
        }

        [GeneratedRegex(@"^[0-9]{20}$")]
        public static partial Regex AccountNumberRegex();
    }
}
