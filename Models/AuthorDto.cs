using System;
using System.ComponentModel.DataAnnotations;

namespace CourseLibraryApi.Models
{
    public class AuthorDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public int Age { get; set; }

        [Required]
        [MaxLength(50)]
        public string MainCategory { get; set; }
    }
}
