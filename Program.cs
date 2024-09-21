using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var eventgridaudience = builder.Configuration["EventgridAudience"];
var eventgridtenant = builder.Configuration["EventgridTenantID"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("eventgrid", options =>
                {
                    options.Audience = eventgridaudience;
                    options.Authority = $"https://login.microsoftonline.com/{eventgridtenant}";
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidAudience = eventgridaudience,
                        ValidIssuer = $"https://login.microsoftonline.com/{eventgridtenant}/v2.0",
                    };
                });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
