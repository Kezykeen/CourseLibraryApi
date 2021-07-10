using System.ComponentModel.DataAnnotations;
using CourseLibraryApi.Models;

namespace CourseLibraryApi.ValidationAttributes
{
    public class EnsureTitleAndDescriptionAreDifferent : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var courseForCreationDto = validationContext.ObjectInstance as CourseForManipulationDto;

            return courseForCreationDto.Title == courseForCreationDto.Description ? 
                new ValidationResult(ErrorMessage, new []{nameof(CourseForCreationDto) }) 
                : ValidationResult.Success;
        }
    }
}
