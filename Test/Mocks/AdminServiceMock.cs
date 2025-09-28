using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Interfaces;

namespace Test.Mocks
{
    public class AdminServiceMock : IAdminServices
    {
        private static List<Admin> admins = new List<Admin>()
        {
            new Admin {
                Id = 1,
                Email = "adm@teste.com",
                Senha = "123456",
                Perfil = "Adm"
            },
            new Admin {
                Id = 2,
                Email = "editor@teste.com",
                Senha = "123456",
                Perfil = "Editor"
            }
        };
        public Admin? IdSearch(int id)
        {
            return admins.Find(a => a.Id == id);
        }

        public Admin Include(Admin admin)
        {
            admin.Id = admins.Count() + 1;
            admins.Add(admin);

            return admin;
        }

        public Admin? Login(LoginDTO loginDTO)
        {
            return admins.Find(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
        }

        public List<Admin> Todos(int? pagina)
        {
            return admins;
        }
    }
}