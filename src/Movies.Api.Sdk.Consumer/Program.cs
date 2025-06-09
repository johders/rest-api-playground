
using System.Text.Json;
using Movies.Api.Sdk;
using Movies.Contracts.Requests;
using Refit;

var moviesApi = RestService.For<IMoviesApi>("http://localhost:5296");

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