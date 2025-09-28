using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enums;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;
using MinimalApi.Infrastructure.Interfaces;

namespace MinimalApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
        }

        private string key = "";

        public IConfiguration Configuration { get; set; } = default!;

        public void ConfigurationServices(IServiceCollection services)
        {
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

            services.AddAuthorization();

            services.AddScoped<IAdminServices, AdminServices>();
            services.AddScoped<IVehicleServices, VehicleServices>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o Token JWT aqui"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            services.AddDbContext<ContextDb>(options =>
            {
                options.UseMySql(
                    Configuration.GetConnectionString("MySql"),
                    ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
                );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion

                #region Admins
                string GenerateJwtToken(Admin admin)    
                {
                    if (string.IsNullOrEmpty(key)) return string.Empty;
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>()
                    {
                        new Claim("Email", admin.Email),
                        new Claim("Perfil", admin.Perfil),
                        new Claim(ClaimTypes.Role, admin.Perfil)
                    };
                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }

                endpoints.MapPost("/admins/login", ([FromBody] LoginDTO loginDTO, IAdminServices adminServices) =>
                {
                    var adm = adminServices.Login(loginDTO);
                    if (adm != null)
                    {
                        string token = GenerateJwtToken(adm);
                        return Results.Ok(new AdmLogado
                        {
                            Email = adm.Email,
                            Perfil = adm.Perfil,
                            Token = token
                        });
                    }
                    else return Results.Unauthorized();
                }).AllowAnonymous().WithTags("Admins");

                endpoints.MapGet("/admins", ([FromQuery] int? pagina, IAdminServices adminServices) =>
                {
                    var adms = new List<AdminModelView>();
                    var admins = adminServices.Todos(pagina);
                    foreach (var adm in admins)
                    {
                        adms.Add(new AdminModelView
                        {
                            Id = adm.Id,
                            Email = adm.Email,
                            Perfil = adm.Perfil
                        });
                    }
                    return Results.Ok(adms);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Admins");

                endpoints.MapGet("/admins/{id}", ([FromRoute] int id, IAdminServices adminServices) =>
                {
                    var admin = adminServices.IdSearch(id);

                    if (admin == null) return Results.NotFound();

                    return Results.Ok(new AdminModelView
                    {
                        Id = admin.Id,
                        Email = admin.Email,
                        Perfil = admin.Perfil
                    });
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Admins");

                endpoints.MapPost("/admins", ([FromBody] AdminDTO adminDTO, IAdminServices adminServices) =>
                {
                    var validation = new ValidationErrors
                    {
                        Messages = new List<string>()
                    };

                    if (string.IsNullOrEmpty(adminDTO.Email))
                        validation.Messages.Add("Email não pode ser vazio.");
                        
                    if (string.IsNullOrEmpty(adminDTO.Senha))
                        validation.Messages.Add("Senha não pode ser vazia.");

                    if (adminDTO.Perfil == null)
                        validation.Messages.Add("Perfil não pode ser vazio.");
                    
                    if (validation.Messages.Count > 0)
                        return Results.BadRequest(validation);

                    var admin = new Admin
                    {
                        Email = adminDTO.Email,
                        Senha = adminDTO.Senha,
                        Perfil = adminDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                    };
                    adminServices.Include(admin);

                    return Results.Created($"/admins/{admin.Id}", (new AdminModelView
                    {
                        Id = admin.Id,
                        Email = admin.Email,
                        Perfil = admin.Perfil
                    }));
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Admins");
                #endregion

                #region Vehicles
                ValidationErrors validationDTO(VehicleDTO vehicleDTO)
                {
                    var validation = new ValidationErrors
                    {
                        Messages = new List<string>()
                    };

                    if (string.IsNullOrEmpty(vehicleDTO.Nome))
                        validation.Messages.Add("O nome não pode ficar em branco.");

                    if (string.IsNullOrEmpty(vehicleDTO.Marca))
                        validation.Messages.Add("A marca não pode ficar em branco.");

                    if (vehicleDTO.Ano < 1950)
                        validation.Messages.Add("Veículo muito antigo. Aceitamos apenas veículos com anos superiores a 1950.");

                    return validation;
                }

                endpoints.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleServices vehicleServices) =>
                {

                    var validation = validationDTO(vehicleDTO);
                    if (validation.Messages.Count > 0)
                        return Results.BadRequest(validation);


                    var vehicle = new Vehicle
                    {
                        Nome = vehicleDTO.Nome,
                        Marca = vehicleDTO.Marca,
                        Ano = vehicleDTO.Ano
                    };
                    vehicleServices.Include(vehicle);

                    return Results.Created($"/vehicles/{vehicle.Id}", vehicle);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Vehicles");

                endpoints.MapGet("/vehicles", ([FromQuery] int? pagina, IVehicleServices vehicleServices) =>
                {
                    var vehicles = vehicleServices.Todos(pagina);

                    return Results.Ok(vehicles);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Vehicles");

                endpoints.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleServices vehicleServices) =>
                {
                    var vehicle = vehicleServices.IdSearch(id);

                    if (vehicle == null) return Results.NotFound();

                    return Results.Ok(vehicle);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicles");

                endpoints.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleServices vehicleServices) =>
                {
                    var vehicle = vehicleServices.IdSearch(id);

                    if (vehicle == null) return Results.NotFound();
                    
                    var validation = validationDTO(vehicleDTO);
                    if (validation.Messages.Count > 0)
                        return Results.BadRequest(validation);

                    vehicle.Nome = vehicleDTO.Nome;
                    vehicle.Marca = vehicleDTO.Marca;
                    vehicle.Ano = vehicleDTO.Ano;

                    vehicleServices.Update(vehicle);

                    return Results.Ok(vehicle);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicles");

                endpoints.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleServices vehicleServices) =>
                {
                    var vehicle = vehicleServices.IdSearch(id);

                    if (vehicle == null) return Results.NotFound();

                    vehicleServices.Delete(vehicle);

                    return Results.NoContent();
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Vehicles");
                #endregion
            });
        }
    }
}