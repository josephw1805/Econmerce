using System.ComponentModel.DataAnnotations;

namespace Econ.Web;

public class AllowedExtensionsAttribute(string[] extensions) : ValidationAttribute
{
  private readonly string[] _extensions = extensions;
  protected override ValidationResult IsValid(object value, ValidationContext validationContext)
  {
    if (value is FormFile file)
    {
      var extensions = Path.GetExtension(file.FileName);
      if (!_extensions.Contains(extensions.ToLower()))
      {
        return new ValidationResult("This photo extension is not allowed!");
      }
    }
    return ValidationResult.Success;
  }
}
