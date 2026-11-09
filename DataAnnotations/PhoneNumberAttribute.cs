using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Prometeus.Services.DataAnnotations;

/// <summary>
/// Аттрибут валидации номера телефона
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public partial class PhoneNumberAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if(value == null) return false;
        if (value is string strvalue)
        {
            var regex = PhoneRegex();
            return regex.IsMatch(strvalue);
        }
        else
            return false;
    }

    [GeneratedRegex(@"^\+[0-9]{10,15}$")]
    private static partial Regex PhoneRegex();
}
