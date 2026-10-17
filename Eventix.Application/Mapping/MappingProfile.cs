using AutoMapper;
using Eventix.Application.DTOs.Auth;
using Eventix.Application.DTOs.Categories;
using Eventix.Application.DTOs.Equipment;
using Eventix.Application.DTOs.Reservations;
using Eventix.Domain.Entities;

namespace Eventix.Application.Mapping;

/// <summary>AutoMapper configuration between domain entities and DTOs. Entities never leave the API directly.</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryResponseDto>();
        CreateMap<CategoryRequestDto, Category>()
            .ForMember(c => c.Name, o => o.MapFrom(r => r.Name.Trim()));

        // CategoryName is filled by AutoMapper's flattening of Category.Name.
        CreateMap<Equipment, EquipmentResponseDto>();
        CreateMap<EquipmentRequestDto, Equipment>()
            .ForMember(e => e.Name, o => o.MapFrom(r => r.Name.Trim()));

        // The enum Role becomes its name ("Admin"/"Producer"); the password hash is not part of the DTO.
        CreateMap<User, UserResponseDto>();

        // UserFullName and EquipmentName come from AutoMapper's flattening (User.FullName, Equipment.Name); Status becomes its name.
        CreateMap<Reservation, ReservationResponseDto>();
        CreateMap<ReservationItem, ReservationItemResponseDto>();
    }
}
