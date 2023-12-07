using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using System.Data;

namespace Dotnetdudes.Web.Blog.Api.Tests
{
    public class BlogApplication : WebApplicationFactory<Program>
    {
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IDbConnection>(provider =>
                    new NpgsqlConnection("Host=localhost;Database=blogstestdb;Username=dudes;Password=Pa55word"));
            });
        }
    }
}
