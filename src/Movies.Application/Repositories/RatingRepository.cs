
using Movies.Application.Database;
using Dapper;

namespace Movies.Application.Repositories;

public class RatingRepository(IDbConnectionFactory dbConnectionFactory) : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition("""
            SELECT round(avg(r.rating), 1) FROM ratings r
            WHERE movieid = @movieid
            """, new { movieId }, cancellationToken: token));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition("""
                    SELECT round(avg(rating), 1), 
                        (SELECT rating 
                        FROM ratings 
                        WHERE movieid = @movieid and userid = @userid 
                        LIMIT 1) 
                    FROM ratings
                    WHERE movieid = @movieid
                    """, new { movieId, userId }, cancellationToken: token));
    }
}
