using Business_Layer.Service;
using Business_Layer.Interface;
using Repository_Layer.Service;
using Repository_Layer.Interface;
using Repository_Layer.Context;
using Repository_Layer.Entity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FluentValidation.AspNetCore;
using Business_Layer.Validation;
using FluentValidation;
using Microsoft.OpenApi.Models;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped <AddressBookBL> ();
builder.Services.AddScoped<IAddressBookBL,AddressBookBL>();
builder.Services.AddScoped<IAddressBookRL,AddressBookRL>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();


builder.Services.AddValidatorsFromAssemblyContaining<AddressBookValidator>();
builder.Services.AddSingleton(new JwtService("ThisIsA32CharLongSecretKey!123456455465869708yyuvhhuvcgguugyft7uyfu78"));
builder.Services.AddSingleton<RedisCacheService>();
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddHostedService<RabbitMQConsumer>();
builder.Services.AddSingleton<RabbitMQPublisher>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AddressBook API",
        Version = "v1",
        Description = "API for managing contacts and users in AddressBookApp",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@example.com"
        }
    });

    // Enable Authorization in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer <token>'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
    // Enable XML comments
    
});




var connectionString = builder.Configuration.GetConnectionString("SqlConnection");

builder.Services.AddDbContext<AddressBookDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
