using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;
using MinimalApi.Infrastructure.Db;
using MinimalApi.Infrastructure.Interfaces;

namespace MinimalApi.Domain.Services
{
    public class VehicleServices : IVehicleServices
    {
        private readonly ContextDb _context;
        public VehicleServices(ContextDb context)
        {
            _context = context;
        }

        public void Delete(Vehicle vehicle)
        {
            _context.Vehicles.Remove(vehicle);
            _context.SaveChanges();
        }

        public Vehicle? IdSearch(int id)
        {
            return _context.Vehicles.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Include(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
        }

        public List<Vehicle> Todos(int? pagina, string? nome = null, string? marca = null)
        {
            var query = _context.Vehicles.AsQueryable();
            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
            }
            int itemsPerPage = 10;

            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itemsPerPage).Take(itemsPerPage);

            return query.ToList();
        }

        public void Update(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
        }
    }
}