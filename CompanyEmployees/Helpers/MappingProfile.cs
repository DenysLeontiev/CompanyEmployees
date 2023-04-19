using AutoMapper;
using Entities;
using Entities.DataTransferObjects;

namespace CompanyEmployees.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>().ForMember(dest => dest.FullAddress, opts => opts.MapFrom(src => string.Concat(src.Address, " , ", src.Country)));
            CreateMap<Employee, EmployeeDto>();
            CreateMap<CompanyForCreationDto, Company>().ForMember(dest => dest.Employees, opts => opts.MapFrom(src => src.Employees));
            CreateMap<EmployeeForCreationDto, Employee>();
            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();
            CreateMap<CompanyForUpdateDto, Company>();
        }
    }
}
