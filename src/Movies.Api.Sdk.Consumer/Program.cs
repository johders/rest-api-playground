
using System.Text.Json;
using Movies.Api.Sdk;
using Refit;

var moviesApi = RestService.For<IMoviesApi>("http://localhost:5296");

var movie = await moviesApi.GetMovieAsync("crimson-phase-2015");

Console.WriteLine(JsonSerializer.Serialize(movie));
Console.ReadLine();