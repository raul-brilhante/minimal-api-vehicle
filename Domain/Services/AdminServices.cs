using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;
using MinimalApi.Infrastructure.Interfaces;

namespace MinimalApi.Domain.Services
{
    public class AdminServices : IAdminServices
    {
        private readonly ContextDb _context;
        public AdminServices(ContextDb context)
        {
            _context = context;
        }

        public Admin Include(Admin admin)
        {
            _context.Admins.Add(admin);
            _context.SaveChanges();

            return admin;
        }

        public Admin? Login(LoginDTO loginDTO)
        {
            var adm = _context.Admins.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }

        public List<Admin> Todos(int? pagina)
        {
            var query = _context.Admins.AsQueryable();
            int itemsPerPage = 10;

            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itemsPerPage).Take(itemsPerPage);

            return query.ToList();
        }

        public Admin? IdSearch(int id)
        {
            return _context.Admins.Where(a => a.Id == id).FirstOrDefault();
        }
    }
}