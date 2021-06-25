using System;
using CourseLibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibraryApi.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository)
        {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
        }

        [HttpGet]
        public IActionResult GetAuthors()
        {
            var authorsFromRepo = _courseLibraryRepository.GetAuthors();
            return Ok(authorsFromRepo);
        }

        [HttpPost("{authorId}")]
        public IActionResult GetAuthor(Guid id)
        {
            var author = _courseLibraryRepository.GetAuthor(id);
            if (author == null)
            {
                return NotFound();
            }
            return Ok(author);
        }
    }
}
