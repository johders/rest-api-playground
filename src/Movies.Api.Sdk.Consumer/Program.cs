
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Refit;

//var moviesApi = RestService.For<IMoviesApi>("http://localhost:5296");

var services = new ServiceCollection();

services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(s => new RefitSettings
        {
            AuthorizationHeaderValueGetter = async (request, token) => await s.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
        })
        .ConfigureHttpClient(x =>
            x.BaseAddress = new Uri("http://localhost:5296"));

var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

var movie = await moviesApi.GetMovieAsync("crimson-phase-2015");

var request = new GetAllMoviesRequest
{
    Title = null,
    YearOfRelease = null,
    SortBy = "-title",
    Page = 1,
    PageSize = 4
};

var movies = await moviesApi.GetMoviesAsync(request);

foreach (var item in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(item));
}

Console.WriteLine("--------------------------------------");
Console.WriteLine(JsonSerializer.Serialize(movie));
Console.ReadLine();