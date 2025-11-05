using AutoMapper;
using DataAnnotations.Models;
using DataAnnotations.Services;
using Microsoft.AspNetCore.Mvc;
using Repositories.Data;

namespace DataAnnotations.Data;

public class DapperService(IDapperRepository repository, IMapper mapper) : IDapperService
{
    public async Task<EventRegistrationDTO> CreateEventRegistrationAsync(EventRegistrationDTO eventRegistrationDTO)
    {
        // var eventRegistration = new EventRegistration
        // {
        //     FullName = eventRegistrationDTO.FullName,
        //     Email = eventRegistrationDTO.Email,
        //     EventName = eventRegistrationDTO.EventName,
        //     EventDate = eventRegistrationDTO.EventDate,
        //     DaysAttending = eventRegistrationDTO.DaysAttending
        // };
        var eventRegistration = mapper.Map<EventRegistration>(eventRegistrationDTO);

        var result = await repository.CreateEventRegistrationAsync(eventRegistration);

        return new EventRegistrationDTO
        {
            Id = result.Id,
            FullName = result.FullName,
            Email = result.Email,
            EventName = result.EventName,
            EventDate = result.EventDate,
            ConfirmEmail = result.Email,
            DaysAttending = result.DaysAttending
        };
    }

    public async Task<EventRegistrationForValidationDTO> CreateEventRegistrationAsync(EventRegistrationForValidationDTO eventRegistrationDTO)
    {
        var eventRegistration = new EventRegistration
        {
            FullName = eventRegistrationDTO.FullName,
            Email = eventRegistrationDTO.Email,
            EventName = eventRegistrationDTO.EventName,
            EventDate = eventRegistrationDTO.EventDate,
            DaysAttending = eventRegistrationDTO.DaysAttending
        };

        var result = await repository.CreateEventRegistrationAsync(eventRegistration);
    
        return new EventRegistrationForValidationDTO
        {
            Id = result.Id,
            FullName = result.FullName,
            Email = result.Email,
            EventName = result.EventName,
            EventDate = result.EventDate,
            ConfirmEmail = result.Email,
            DaysAttending = result.DaysAttending
        };
    }

    public async Task<EventRegistrationDTO?> GetEventRegistrationByIdAsync(int id)
    {
        var eventRegistration = await repository.GetEventRegistrationByIdAsync(id);
        if (eventRegistration == null) return null;

        return mapper.Map<EventRegistrationDTO>(eventRegistration);
    }

    public async Task<PagedResult<EventRegistrationDTO>> GetEventRegistrationsAsync(int pageSize, int lastId, IUrlHelper urlHelper)
    {
        var (eventRegistrations, hasNextPage) = await repository.GetEventRegistrationsAsync(pageSize, lastId);

        var items = eventRegistrations.Select(e => new EventRegistrationDTO
        {
            Id = e.Id,
            FullName = e.FullName,
            Email = e.Email,
            EventName = e.EventName,
            EventDate = e.EventDate,
            ConfirmEmail = e.Email,
            DaysAttending = e.DaysAttending
        }).ToList().AsReadOnly();

        var hasPreviousPage = lastId > 0;
        var previousPageUrl = hasPreviousPage
            ? urlHelper.Action("GetEventRegistrations", new { pageSize, lastId = items.First().Id })
            : null;
        var nextPageUrl = hasNextPage
            ? urlHelper.Action("GetEventRegistrations", new { pageSize, lastId = items.Last().Id })
            : null;

        return new PagedResult<EventRegistrationDTO>
        {
            Items = items,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage,
            PreviousPageUrl = previousPageUrl,
            NextPageUrl = nextPageUrl,
            PageSize = pageSize
        };
    }

    public async Task UpdateEventRegistrationAsync(EventRegistrationDTO eventRegistrationDto)
    {
        var eventRegistration = mapper.Map<EventRegistration>(eventRegistrationDto);
        await repository.UpdateEventRegistrationAsync(eventRegistration);
    }
}