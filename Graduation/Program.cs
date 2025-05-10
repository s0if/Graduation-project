
using Graduation.Data;
using Graduation.Model;
using Graduation.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using testAPI_crud.Model;
using Scalar.AspNetCore;

namespace Graduation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<ApplicationDbContext>(option =>
                option.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnections"),
                     sqlServerOptions => sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                     )
            );
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(option =>
            {
                option.Password.RequireLowercase = true;
                option.Password.RequireUppercase = true;
                option.Password.RequireDigit = true;
                option.Password.RequireNonAlphanumeric = true;
                option.User.RequireUniqueEmail = false;
                option.SignIn.RequireConfirmedEmail = false;
                option.SignIn.RequireConfirmedPhoneNumber = false;

            }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddScoped<AuthServices>();
            builder.Services.AddScoped<ExtractClaims>();
            builder.Services.AddAuthentication(
             options =>
             {
                 options.DefaultAuthenticateScheme = "Bearer";
                 options.DefaultChallengeScheme = "Bearer";
             }
             ).AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new
                Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ClockSkew = TimeSpan.Zero ,
                     
                     //from model AuthServices
                     ValidIssuer = "GRADUATHION PROJECT",
                     ValidAudience = "GRADUATHION",
                     ValidateLifetime = true,
                     IssuerSigningKey = new
                SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("jwt")["secretkey"
                ]))
                 };
             }
             );

            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("allowAll", build => 
                {
                    build.WithOrigins("http://localhost:5173", "http://localhost:5100", "https://aqar-mj6h.onrender.com")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            //builder.Services.AddIdentity<ApplicationUser,IdentityRole<int>>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();
            var app = builder.Build();

            // Configure the HTTP request pipeline.

            //if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(option =>
                {
                    option.RouteTemplate = "openapi/{documentName}.json";
                });
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors("allowAll");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.MapHub<ChatHub>("/chatHub");
            app.Run();
        }
    }
}
