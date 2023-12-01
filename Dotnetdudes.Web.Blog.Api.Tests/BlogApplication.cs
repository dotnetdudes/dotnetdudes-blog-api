using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Data;

namespace Dotnetdudes.Web.Blog.Api.Tests
{
    public class BlogApplication : WebApplicationFactory<Program>
    {
        private readonly Mock<IDbConnection> _mockDbConnection = new Mock<IDbConnection>();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
            });
        }
    }
}
