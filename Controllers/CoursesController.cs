using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibraryApi.Entities;
using CourseLibraryApi.Models;
using CourseLibraryApi.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibraryApi.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                                       throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCourses(Guid authorId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            return Ok(_mapper.Map<IEnumerable<CourseDto>>(_courseLibraryRepository.GetCourses(authorId)));
        }

        [HttpGet("{courseId}", Name = "GetCourse")]
        public ActionResult<CourseDto> GetCourse(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var course = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (course == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(course));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourse(Guid authorId, CourseForCreationDto courseDto)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            if (courseDto == null)
            {
                return NotFound();
            }

            var entityToCreate = _mapper.Map<Course>(courseDto);

            _courseLibraryRepository.AddCourse(authorId, entityToCreate);
            _courseLibraryRepository.Save();

            return CreatedAtRoute("GetCourse", new {authorId, courseId = entityToCreate.Id},
                _mapper.Map<CourseDto>(entityToCreate));
        }

        [HttpPatch("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, [FromBody] JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromDb = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseForAuthorFromDb == null)
            {
                return NotFound();
            }

            var courseToPatch = _mapper.Map<CourseForUpdateDto>(courseForAuthorFromDb);
            
            patchDocument.ApplyTo(courseToPatch, ModelState);
            if (!TryValidateModel(courseToPatch))
            {
               return ValidationProblem(ModelState);
            }

            _mapper.Map(courseToPatch, courseForAuthorFromDb);
            _courseLibraryRepository.UpdateCourse(courseForAuthorFromDb);
            _courseLibraryRepository.Save();

            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourse(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var course = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (course == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteCourse(course);
            _courseLibraryRepository.Save();

            return NoContent();
        } 

        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}
