using Dotnetdudes.Web.Blog.Api;
using Dotnetdudes.Web.Blog.Api.Routes;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using Serilog;
using Serilog.Events;
using System.Data;

Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
   .Enrich.FromLogContext()
   .WriteTo.Console()
   .CreateBootstrapLogger();

Log.Information("Starting Dotnetdudes Blog Api application");
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
     .ReadFrom.Configuration(context.Configuration)
     .ReadFrom.Services(services)
     .Enrich.FromLogContext());

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

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    DbInitialiser.Initialise(app);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages(async statusCodeContext 
    => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
                 .ExecuteAsync(statusCodeContext.HttpContext));

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => {
            var error = context?.Features?.Get<IExceptionHandlerFeature>()?.Error;
            if(error is not null)
            {
                Log.Error(error, "Unhandled exception");
            }            
            await Results.Problem().ExecuteAsync(context!);
        })
    ) ;

// todoV1 endpoints
app.MapGroup("/posts/v1")
    .MapBlogEndpointsV1()
    .WithTags("Blog V1 Endpoints");

app.Run();

public partial class Program { }
