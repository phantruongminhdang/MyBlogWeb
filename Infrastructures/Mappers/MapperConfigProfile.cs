using Application.Commons;
using Application.ModelViews.BlogTypeViewModels;
using Application.ModelViews.BlogViewModels;
using Application.ModelViews.UserViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructures.Mappers
{
    public class MapperConfigProfile : Profile
    {
        public MapperConfigProfile() {
            CreateMap(typeof(Pagination<>), typeof(Pagination<>));
            CreateMap<BlogModel, Blog>();
            CreateMap<BlogTypeModel, BlogType>();
            CreateMap<UserViewModel, ApplicationUser>().ReverseMap();
            CreateMap<UserViewForUserRoleModel, ApplicationUser>().ReverseMap();
        }
    }
}
