using System.Text.Json;
using Dotnetdudes.Web.Blog.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Dotnetdudes.Web.Blog.Api.Tests;

public class BlogApplicationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BlogApplicationTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    // test the DbInitialiser class can connect to the database

}
