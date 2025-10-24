using Vlab.Dominio.DTOs;
using Microsoft.AspNetCore.Builder;
using Vlab.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using Vlab.Dominio.Interfaces;
using Vlab.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using Vlab.Dominio.Entidades;
using Vlab.Dominio.Enuns;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Vlab.Dominio.ModelViews;
using Vlab.Dominio.Servicos.Externos;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key))
{
	key = "Vlab";
}

builder.Services.AddAuthentication(option =>
{
	option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateLifetime = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
		ValidateIssuer = false,
		ValidateAudience = false,
	};
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddHttpClient<IOpenLibraryServico, OpenLibraryService>();
builder.Services.AddScoped<IUsuarioServico, UsuarioServico>();
builder.Services.AddScoped<IMaterialServico, MaterialServico>();
builder.Services.AddScoped<IAutorServico, AutorServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Insira o token JWT aqui"
	});

	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
        
	});
});


builder.Services.AddDbContext<DbContexto>(
	options => options.UseMySql(
		builder.Configuration.GetConnectionString("MySql"),
		ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
	));


var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion


#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion
