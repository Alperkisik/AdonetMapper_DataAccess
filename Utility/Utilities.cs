using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AdonetMapper_DataAccess.Utility
{
    public class Utilities
    {
        public static Dictionary<string, object> SqlParameters_FromObject(object dtoObject)
        {
            var _parameters = new Dictionary<string, object>();

            var properties = dtoObject.GetType().GetProperties();

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(dtoObject);
                if (propertyValue == null) continue;



                var propertyType = property.PropertyType;
                if (propertyType == typeof(string)) _parameters.Add($"@{property.Name}", propertyValue);
                else _parameters.Add($"@{property.Name}", propertyValue);
            }

            return _parameters;
        }

        public static Result Validation(object dtoObject)
        {
            var properties = dtoObject.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(dtoObject);
                var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();

                foreach (var attribute in validationAttributes)
                {
                    var result = attribute.GetValidationResult(value, new ValidationContext(dtoObject));

                    if (result != ValidationResult.Success)
                        return Result.Failure($"Alan Adı: {property.Name}{result.ErrorMessage.Replace(dtoObject.GetType().Name, "")}");
                }
            }

            return Result.Success();
        }
    }
}
