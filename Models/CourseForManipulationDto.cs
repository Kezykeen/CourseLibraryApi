using System.ComponentModel.DataAnnotations;
using CourseLibraryApi.ValidationAttributes;

namespace CourseLibraryApi.Models
{
    [EnsureTitleAndDescriptionAreDifferent(ErrorMessage = "Title and Description must be different")]
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "Please enter a title")]
        [MaxLength(100, ErrorMessage = "Title shouldn't have more than 100 characters")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "Description shouldn't have more than 1500 characters")]
        public virtual string Description { get; set; }
    }
}
