using System.ComponentModel.DataAnnotations;

namespace CourseLibraryApi.Models
{
    public class CourseForCreationDto : CourseForManipulationDto
    {
        [Required(ErrorMessage = "Please enter a description")]
        public override string Description { get; set; }
    }
}
