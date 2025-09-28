using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Services;
using MinimalApi.Infrastructure.Db;

namespace Test.Domain.Entities;

[TestClass]
public class AdminServiceTest
{
    private ContextDb CreateTestContext()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new ContextDb(configuration);
    }

    [TestMethod]
    public void TestCreateAdmin()
    {
        var context = CreateTestContext();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE admins");
        var adm = new Admin();

        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "testesenha";
        adm.Perfil = "Adm";

        var adminService = new AdminServices(context);

        adminService.Include(adm);

        Assert.AreEqual(1, adminService.Todos(1).Count());
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("testesenha", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);
    }

    [TestMethod]
    public void TestSearchAdmin()
    {
        var context = CreateTestContext();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE admins");
        var adm = new Admin();

        adm.Email = "teste@teste.com";
        adm.Senha = "testesenha";
        adm.Perfil = "Adm";

        var adminService = new AdminServices(context);

        adminService.Include(adm);
        var admDB = adminService.IdSearch(adm.Id);

        Assert.AreEqual(1, admDB?.Id);
    }
}