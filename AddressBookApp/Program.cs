using Business_Layer.Service;
using Business_Layer.Interface;
using Repository_Layer.Service;
using Repository_Layer.Interface;
using Repository_Layer.Context;
using Repository_Layer.Entity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IAddressBookBL,AddressBookBL>();
builder.Services.AddScoped<IAddressBookRL,AddressBookRL>();


var connectionString = builder.Configuration.GetConnectionString("SqlConnection");

builder.Services.AddDbContext<AddressBookDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
