using AutoMapper;
using CourseLibraryApi.Entities;
using CourseLibraryApi.Helpers;
using CourseLibraryApi.Models;

namespace CourseLibraryApi.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Author, AuthorDto>()
                .ForMember(dest=>dest.Name,
                    opt => opt.MapFrom(src=> $"{src.FirstName} {src.LastName}"))
                .ForMember(dest=>dest.Age,
                    opt=>opt.MapFrom(src=>src.DateOfBirth.GetCurrentAge()));

            CreateMap<AuthorForCreationDto, Author>();

            CreateMap<Course, CourseDto>().ReverseMap();

            CreateMap<CourseForCreationDto, Course>();

            CreateMap<CourseForUpdateDto, Course>().ReverseMap();
        }
    }
}
