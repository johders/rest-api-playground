using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Ratings;
public static class GetUserRatingsEndpoint
{
    public const string Name = "GetUserRating";

    public static IEndpointRouteBuilder MapGetUserRatings(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Ratings.GetUserRatings, async (
                IRatingService ratingService, IRatingService ratingService1,
                HttpContext context, CancellationToken token) =>
            {
                var userId = context.GetUserId();
                var ratings = await ratingService.GetRatingsForUserAsync(userId!.Value, token);
                var ratingsResponse = ratings.MapToResponse();

                return TypedResults.Ok(ratingsResponse);
            })
            .WithName(Name)
            .Produces<MovieRatingResponse>(StatusCodes.Status200OK)
            .RequireAuthorization()
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0);
        return app;
    }
}