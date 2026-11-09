using System.ComponentModel.DataAnnotations;

namespace Prometeus.Services.DataAnnotations
{
    /// <summary>
    /// Аттрибут валидации допускающий только положительные числа
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NonZeroAttribute() : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value switch
            {
                null => false,

                byte b => b > 0,
                sbyte sb => sb > 0,

                short s => s > 0,
                ushort us => us > 0,

                int i => i > 0,
                uint ui => ui > 0,
                
                long l => l > 0,
                ulong ul => ul > 0,

                decimal m => m > 0,

                double d => d > 0,
                float f => f > 0,

                _ => base.IsValid(value)
            };
        }
    }
}
