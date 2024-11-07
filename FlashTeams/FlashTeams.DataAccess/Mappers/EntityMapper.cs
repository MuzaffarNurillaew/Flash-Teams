using AutoMapper;
using FlashTeams.Domain.Entities;

namespace FlashTeams.DataAccess.Mappers;
public class EntityMapper : Profile
{
    public EntityMapper()
    {
        CreateMap<User, User>();
    }
}