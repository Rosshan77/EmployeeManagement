using AutoMapper;
using EmployeeManagementDAL.Models;
using EmployeeManagementModel.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EmployeeManagementBLL.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Employee mappings
            CreateMap<Employee, EmployeeDTO>().ReverseMap();

            // Auth mappings
            CreateMap<RegisterDTO, AppUser>();
            CreateMap<AppUser, LoginResponseDTO>();
        }
    }
}