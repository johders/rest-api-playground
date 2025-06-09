
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Contracts.Requests;
using Refit;

//var moviesApi = RestService.For<IMoviesApi>("http://localhost:5296");

var services = new ServiceCollection();

services.AddRefitClient<IMoviesApi>(x => new RefitSettings
    {
        AuthorizationHeaderValueGetter = (request, token) => Task.FromResult("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI3YjRiMjczMS1lMjlmLTRmNDMtOTE5Yi1jOGUzZDdkM2Q1MzkiLCJzdWIiOiJuaWNrQG5pY2tjaGFwc2FzLmNvbSIsImVtYWlsIjoibmlja0BuaWNrY2hhcHNhcy5jb20iLCJ1c2VyaWQiOiJkODU2NmRlMy1iMWE2LTRhOWItYjg0Mi04ZTM4ODdhODJlNDEiLCJhZG1pbiI6dHJ1ZSwidHJ1c3RlZF9tZW1iZXIiOnRydWUsIm5iZiI6MTc0OTQ0NzA4NiwiZXhwIjoxNzQ5NDc1ODg2LCJpYXQiOjE3NDk0NDcwODYsImlzcyI6Imh0dHBzOi8vaWQudGVzdGVyLmNvbSIsImF1ZCI6Imh0dHBzOi8vbW92aWVzLnRlc3Rlci5jb20ifQ.505pRM5q2RQ4WHUXwJRNi9OvOYgJ33jEUfeyPKlkbLs")
    })
    .ConfigureHttpClient(x => x.BaseAddress = new Uri("http://localhost:5296"));

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