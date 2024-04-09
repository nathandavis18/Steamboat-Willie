using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Utility
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DateValidatorAttribute : ValidationAttribute
    {
        public DateValidatorAttribute(){}
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            DateTime dateValue;
            var date = DateTime.TryParse(value.ToString(), out dateValue);
            if(dateValue > DateTime.Now)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);            
        }

        public override string FormatErrorMessage(string name = "Error")
        {
            return base.FormatErrorMessage(name);
        }
    }
}
