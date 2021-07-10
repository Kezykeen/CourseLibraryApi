using System;
using System.Collections.Generic;
using CourseLibraryApi.Entities;

namespace CourseLibraryApi.Models
{
    public class AuthorForCreationDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        public string MainCategory { get; set; }

        public ICollection<Course> Courses { get; set; }
            = new List<Course>();
    }
}
