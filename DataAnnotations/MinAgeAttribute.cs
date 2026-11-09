using System.ComponentModel.DataAnnotations;
using TimeMachine.Core;

namespace Prometeus.Services.DataAnnotations;

/// <summary>
/// Атрибут валидации минимального возраста для <see cref="DateOnly"/>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class MinAgeAttribute : ValidationAttribute
{
    public int MinAge { get; }

    public MinAgeAttribute(int minAge)
    {
        if (minAge < 0)
            throw new ArgumentOutOfRangeException(nameof(minAge), "Минимальный возраст не может быть отрицательным");
        MinAge = minAge;
    }
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is not DateOnly date)
            return false;

        var today = DateOnly.FromDateTime(SharedTimeProvider.Read(DateTime.UtcNow));
        var age = CalculateAge(date, today);
        return age >= MinAge;
    }

    public override string FormatErrorMessage(string name)
    {
        return ErrorMessage ?? $"Возраст должен быть не менее {MinAge} лет";
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;

        if (today < birthDate.AddYears(age))
            age--;

        return age;
    }
}
