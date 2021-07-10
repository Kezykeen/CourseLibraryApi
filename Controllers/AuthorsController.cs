using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CourseLibraryApi.Entities;
using CourseLibraryApi.Helpers;
using CourseLibraryApi.Models;
using CourseLibraryApi.ResourceParameters;
using CourseLibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibraryApi.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper;
        }

        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<Author>> GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            var authorsFromRepo = _courseLibraryRepository.GetAuthors(authorsResourceParameters);
             
            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        public ActionResult<Author> GetAuthor(Guid authorId)
        {
            var author = _courseLibraryRepository.GetAuthor(authorId);
            
            if (author == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<AuthorDto>(author));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto authorDto)
        {
            var entityToCreate = _mapper.Map<Author>(authorDto);

            _courseLibraryRepository.AddAuthor(entityToCreate);
            _courseLibraryRepository.Save();

            return CreatedAtRoute("GetAuthor", new {authorId = entityToCreate.Id},
                _mapper.Map<AuthorDto>(entityToCreate));
        }

        [HttpGet("/api/authorCollections/({ids})", Name = "GetAuthorCollection")]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthorCollection(
            [FromRoute] [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorsCollection = _courseLibraryRepository.GetAuthors(ids);

            if (ids.Count() != authorsCollection.Count())
            {
                return NotFound();
            }

            var authorsDto = _mapper.Map<IEnumerable<AuthorDto>>(authorsCollection);

            return Ok(authorsDto);
        }

        [HttpPost("/api/authorCollections")]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(IEnumerable<AuthorForCreationDto> authorsCollection)
        {
            if (authorsCollection == null)
            {
                return BadRequest();
            }

            var authors = _mapper.Map<IEnumerable<Author>>(authorsCollection);
            foreach (var author in authors)
            {
                _courseLibraryRepository.AddAuthor(author);
            }

            _courseLibraryRepository.Save();
            var authorsDto = _mapper.Map<IEnumerable<AuthorDto>>(authors);
            var stringIds = string.Join(",", authorsDto.Select(x => x.Id));

            return CreatedAtRoute("GetAuthorCollection", new {ids = stringIds} , authorsDto);
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var author = _courseLibraryRepository.GetAuthor(authorId);
            if (author == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteAuthor(author);
            _courseLibraryRepository.Save();

            return NoContent();
        }
    }
}
