using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;

namespace MinimalApi.Infrastructure.Interfaces
{
    public interface IAdminServices
    {
        Admin? Login(LoginDTO loginDTO);
        Admin Include(Admin admin);
        List<Admin> Todos(int? pagina);
        Admin? IdSearch(int id);
    }
}