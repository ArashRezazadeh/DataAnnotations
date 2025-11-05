using AutoMapper;
using DataAnnotations.Data;
using DataAnnotations.Models;

namespace events.Models;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<EventRegistrationDTO, EventRegistration>().ReverseMap();
         CreateMap<EventRegistrationForValidationDTO, EventRegistration>()
            .ForMember(dest => dest.AdditionalContact, opt => opt.MapFrom(src => src.AdditionalContact));
        CreateMap<AdditionalContactInfoDTO, AdditionalContactInfo>();
        CreateMap<EventRegistration, EventRegistrationForValidationDTO>()
            .ForMember(dest => dest.AdditionalContact, opt => opt.MapFrom(src => src.AdditionalContact));
        CreateMap<AdditionalContactInfo, AdditionalContactInfoDTO>();
    }
}