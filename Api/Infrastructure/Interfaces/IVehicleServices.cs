using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Domain.Entities;

namespace MinimalApi.Infrastructure.Interfaces
{
    public interface IVehicleServices
    {
        List<Vehicle> Todos(int? pagina, string? nome = null, string? marca = null);
        Vehicle? IdSearch(int id);
        void Include(Vehicle vehicle);
        void Update(Vehicle vehicle);
        void Delete(Vehicle vehicle);
    }
}