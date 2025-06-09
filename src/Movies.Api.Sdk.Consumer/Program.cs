
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

//POST create new movie
// var newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
// {
//     Title = "ZJungle book II",
//     YearOfRelease = 1991,
//      Genres = new[] { "Adventure", "Drama" }
// });

//PUT update movie
// var updatedMovie = await moviesApi.UpdateMovieAsync(Guid.Parse("2ec106a9-bcfa-47f7-8713-5157ae486d54"), new UpdateMovieRequest
// {
//     Title = "ZJungler book II",
//     YearOfRelease = 1993,
//     Genres = new[] { "Sci-Fi" }
// });

//DELETE movie
//await moviesApi.DeleteMovieAsync(Guid.Parse("3e22b621-c331-4aa6-9b25-d60833534d60"));

//GET by slug
//var movie = await moviesApi.GetMovieAsync("void-memory-2008");

//PUT rate movie
//await moviesApi.RateMovieAsync(Guid.Parse("30d74bb7-ef56-4e8e-b1e0-373528cbe3d4"), new RateMovieRequest { Rating = 4 });

//GET user ratings
//var ratings = await moviesApi.GetUserRatingsAsync();
//Console.WriteLine(JsonSerializer.Serialize(ratings));

//DELETE rating
//await moviesApi.DeleteRatingAsync(Guid.Parse("8a0db90e-ac53-4c5a-83c8-43aa645abfff"));

var request = new GetAllMoviesRequest
{
    Title = null,
    YearOfRelease = null,
    SortBy = "-title",
    Page = 1,
    PageSize = 4
};

//GET all movies
var movies = await moviesApi.GetMoviesAsync(request);

foreach (var item in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(item));
}

Console.WriteLine("--------------------------------------");
//Console.WriteLine(JsonSerializer.Serialize(movie));
Console.ReadLine();