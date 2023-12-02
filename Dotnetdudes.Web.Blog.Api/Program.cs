using Dotnetdudes.Web.Blog.Api;
using Dotnetdudes.Web.Blog.Api.Routes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add authentication with JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001";
        options.Audience = "Dotnetdudes.Web.Blog.Api";
    });

// add postgressql database connection
builder.Services.AddScoped<IDbConnection>(provider =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    DbInitialiser.Initialise(app);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(exceptionHandlerApp 
    => exceptionHandlerApp.Run(async context 
        => await Results.Problem()
                     .ExecuteAsync(context)));

// todoV1 endpoints
app.MapGroup("/posts/v1")
    .MapBlogEndpointsV1()
    .WithTags("Blog V1 Endpoints");

app.Run();

public partial class Program { }
