﻿using AutoMapper;
using MediCase.WebAPI.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace MediCase.WebAPI.MapperProfiles
{
    public class EntitiesGraphDataProfile : Profile
    {
        public EntitiesGraphDataProfile()
        {
            CreateMap<Entities.Moderator.Entity, EntityGraphObjectDto>()
                .ForMember(dest => dest.Childs, opt => opt.MapFrom(x => x.EntitiesGraphDatumParents))
                .ForSourceMember(src => src.EntitiesGraphDatumChildren, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.Type, opt => opt.DoNotValidate())
                .ForMember(dest => dest.HasChilds, opt => opt.MapFrom(src => src.HasChilds))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => (src.LockExpirationDate <= DateTime.UtcNow.AddSeconds(5)) ? false : true));

            CreateMap<Entities.Content.Entity, EntityGraphObjectDto>()
                .ForMember(dest => dest.Childs, opt => opt.MapFrom(x => x.EntitiesGraphDatumParents))
                .ForSourceMember(src => src.EntitiesGraphDatumChildren, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.Type, opt => opt.DoNotValidate())
                .ForMember(dest => dest.HasChilds, opt => opt.MapFrom(src => src.HasChilds));


            CreateMap<Entities.Moderator.EntitiesGraphDatum, EntityWithTranslationDto>()
                .ForSourceMember(src => src.Parent, opt => opt.DoNotValidate())
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.Child.EntityId))
                .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Child.TypeId))
                .ForMember(dest => dest.EntityTranslations, opt => opt.MapFrom(src => src.Child.EntityTranslations))
                .ForMember(dest => dest.EntityOrder, opt => opt.MapFrom(src => src.Child.EntityOrder))
                .ForMember(dest => dest.HasChilds, opt => opt.MapFrom(src => src.Child.HasChilds))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => (src.Child.LockExpirationDate <= DateTime.UtcNow.AddSeconds(5)) ? false : true));

            CreateMap<Entities.Content.EntitiesGraphDatum, EntityWithTranslationDto>()
                .ForSourceMember(src => src.Parent, opt => opt.DoNotValidate())
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.Child.EntityId))
                .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Child.TypeId))
                .ForMember(dest => dest.EntityTranslations, opt => opt.MapFrom(src => src.Child.EntityTranslations))
                .ForMember(dest => dest.EntityOrder, opt => opt.MapFrom(src => src.Child.EntityOrder))
                .ForMember(dest => dest.HasChilds, opt => opt.MapFrom(src => src.Child.HasChilds));
        }
    }
}
