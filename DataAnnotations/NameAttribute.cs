using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Prometeus.Services.DataAnnotations;

/// <summary>
/// Аттрибут для валидации имени
/// </summary>
/// <remarks>Формат: Начинается с заглавной буквы, допускается только кирилица или латиница, апостроф или дефис</remarks>
/// <param name="allowNull">Допускается ли null</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public partial class NameAttribute(bool allowNull = false) : ValidationAttribute
{
    protected bool AllowNull { get; set; } = allowNull;
    public override bool IsValid(object? value)
    {
        if (value == null) return AllowNull;
        if (value is string strvalue)
        {
            var regex = NameRegex();
            return regex.IsMatch(strvalue);
        }
        else
            return false;
    }


    public override string FormatErrorMessage(string name)
    {
        return ErrorMessage ?? $"Невалидное значение для {name}";
    }

    [GeneratedRegex("^[A-ZА-ЯЁ][a-zа-яё']+(?:-[A-ZА-ЯЁ][a-zа-яё']+)*$")]
    private static partial Regex NameRegex();
}

/// <summary>
/// Аттрибут для валидациии фамилии
/// </summary>
/// <remarks>Формат: Начинается с заглавной буквы, допускается только кирилица или латиница, апостроф или дефис</remarks>
public class SurnameAttribute : NameAttribute;

/// <summary>
/// Аттрибут для валидации отчества
/// </summary>
/// <remarks>Формат: Начинается с заглавной буквы, допускается только кирилица или латиница, апостроф или дефис</remarks>
public class PatronimycAttribute() : NameAttribute(true);
