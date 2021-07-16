using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using CourseLibrary.API.ActionConstraints;
using CourseLibraryApi.Entities;
using CourseLibraryApi.Helpers;
using CourseLibraryApi.Models;
using CourseLibraryApi.ResourceParameters;
using CourseLibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CourseLibraryApi.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;
        private readonly IMapper _mapper;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, 
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService,
            IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ?? 
                                       throw new ArgumentNullException(nameof(courseLibraryRepository));
            _propertyMappingService = propertyMappingService ?? 
                                      throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ??
                                      throw new ArgumentNullException(nameof(propertyCheckerService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("", Name = "GetAuthors")]
        [HttpHead]
        public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            var authorsFromRepo = _courseLibraryRepository.GetAuthors(authorsResourceParameters);

            var paginationMetaData = new
            {
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                totalCount = authorsFromRepo.TotalCount
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetaData));

            var links = CreateLinksForAuthors(authorsResourceParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrev);

            var shapedAuthorsCollection = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                .ShapeData(authorsResourceParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthorsCollection.Select(author =>
            {
                var shapedAuthor = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid) shapedAuthor["Id"], null);
                shapedAuthor.Add("links", authorLinks);
                return shapedAuthor;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };

            return Ok(linkedCollectionResource);
        }

        [Produces("application/json",
            "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.full+json",
            "application/vnd.marvin.author.full.hateoas+json",
            "application/vnd.marvin.author.friendly+json",
            "application/vnd.marvin.author.friendly.hateoas+json")]
        [HttpGet("{authorId}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid authorId, string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var author = _courseLibraryRepository.GetAuthor(authorId);
            
            if (author == null)
            {
                return NotFound();
            }

            var includeLinks = parsedMediaType.SubTypeWithoutSuffix
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            IEnumerable<LinkDto> links = new List<LinkDto>();

            if (includeLinks)
            {
                links = CreateLinksForAuthor(authorId, fields);
            }

            var primaryMediaType = includeLinks
                ? parsedMediaType.SubTypeWithoutSuffix
                    .Substring(0, parsedMediaType.SubTypeWithoutSuffix.Length - 8)
                : parsedMediaType.SubTypeWithoutSuffix;

            // full author
            if (primaryMediaType == "vnd.marvin.author.full")
            {
                var fullResourceToReturn = _mapper.Map<AuthorFullDto>(author)
                    .ShapeData(fields) as IDictionary<string, object>;

                if (includeLinks)
                {
                    fullResourceToReturn.Add("links", links);
                }

                return Ok(fullResourceToReturn);
            }

            // friendly author
            var friendlyResourceToReturn = _mapper.Map<AuthorDto>(author)
                .ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                friendlyResourceToReturn.Add("links", links);
            }

            return Ok(friendlyResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        [Consumes("application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        public IActionResult CreateAuthorWithDateOfDeath(AuthorForCreationWithDateOfDeathDto author)
        {
            var authorEntity = _mapper.Map<Entities.Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
                as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor",
                new {authorId = linkedResourceToReturn["Id"]},
                linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthor")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/json",
            "application/vnd.marvin.authorforcreation+json")]
        [Consumes(
            "application/json",
            "application/vnd.marvin.authorforcreation+json")]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto authorDto)
        {
            var entityToCreate = _mapper.Map<Author>(authorDto);

            _courseLibraryRepository.AddAuthor(entityToCreate);
            _courseLibraryRepository.Save();

            var links = CreateLinksForAuthor(entityToCreate.Id, null);
            var authorObject = _mapper.Map<AuthorDto>(entityToCreate).ShapeData(null) as IDictionary<string, object>;

            authorObject.Add("links", links);

            return CreatedAtRoute("GetAuthor", new {authorId = authorObject["Id"]}, authorObject);
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

        [HttpDelete("{authorId}", Name = "DeleteAuthor")]
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

        public string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters,
            ResourceUriTypes types)
        {
            return types switch
            {
                ResourceUriTypes.NextPage => Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchTerm = authorsResourceParameters.SearchTerm
                    }),
                ResourceUriTypes.PreviousPage => Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchTerm = authorsResourceParameters.SearchTerm
                    }),
                ResourceUriTypes.Current => Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchTerm = authorsResourceParameters.SearchTerm
                    }),
                _ => Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchTerm = authorsResourceParameters.SearchTerm
                    })
            };
        }

        public IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields)
        {
            var links = new List<LinkDto>
            {
                string.IsNullOrWhiteSpace(fields)
                    ? new LinkDto(Url.Link("GetAuthor", new {authorId}), "self", "Get")
                    : new LinkDto(Url.Link("GetAuthor", new {authorId, fields}), "self", "Get"),
                new LinkDto(Url.Link("DeleteAuthor", new {authorId}), "delete_author", "Post"),
                new LinkDto(Url.Link("CreateCourseForAuthor", new {authorId}), "create_course_for_author", "Post"),
                new LinkDto(Url.Link("GetCoursesForAuthor", new {authorId}), "get_courses_for_author", "Get")
            };

            return links;
        }

        public IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrev)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriTypes.Current),
                "GetAuthors", "Get"));

            if (hasNext)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriTypes.NextPage),
                    "next_page", "Get"));
            }

            if (hasPrev)
            {
                links.Add(new LinkDto(
                    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriTypes.PreviousPage), "prev_page",
                    "Get"));
            }

            return links;
        }
    }
}
