using System.ComponentModel.DataAnnotations;

namespace Econ.Web;

public class MaxFileSizeAttribute(int maxFileSize) : ValidationAttribute
{
  private readonly int _maxFileSize = maxFileSize;
  protected override ValidationResult IsValid(object value, ValidationContext validationContext)
  {
    if (value is FormFile file)
    {
      if (file.Length > (_maxFileSize * 1024 * 1024))
      {
        return new ValidationResult("Maximum allowed file size is {_maxFileSize} MB");
      }
    }
    return ValidationResult.Success;
  }
}
