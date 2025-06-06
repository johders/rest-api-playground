using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO movies (id, slug, title, year_of_release)
            VALUES (@Id, @Slug, @Title, @YearOfRelease)
            """, movie, cancellationToken: token));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name)
                    """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
            SELECT id, title, slug, year_of_release as YearOfRelease, round(avg(r.rating), 1) AS rating, myr.rating AS userrating
            FROM movies m
            LEFT JOIN ratings r ON m.id = r.movieid
            LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @userId
            WHERE id = @id
            GROUP BY id, userrating
            """, new { id, userId }, cancellationToken: token));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(new CommandDefinition("""
            SELECT name FROM genres WHERE movieId = @id
            """, new { id }, cancellationToken: token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
            SELECT id, title, slug, year_of_release as YearOfRelease, round(avg(r.rating), 1) AS rating, myr.rating AS userrating
            FROM movies m
            LEFT JOIN ratings r ON m.id = r.movieid
            LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @userId
            WHERE slug = @slug
            GROUP BY id, userrating
            """, new { slug, userId }, cancellationToken: token));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(new CommandDefinition("""
            SELECT name FROM genres WHERE movieId = @id
            """, new { movie.Id }, cancellationToken: token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var orderClause = string.Empty;
        if (options.SortField is not null)
        {
            orderClause = $"""
            , m.{options.SortField}
            ORDER BY m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}            
            """;
        }

        var result = await connection.QueryAsync(new CommandDefinition($"""
            SELECT m.*, string_agg(distinct g.name, ',') AS genres, round(avg(r.rating), 1) AS rating, myr.rating AS userrating
            FROM movies m 
            LEFT JOIN genres g ON m.id = g.movieid
            LEFT JOIN ratings r ON m.id = r.movieid
            LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @userId
            WHERE (@title IS NULL OR m.title LIKE ('%' || @title || '%'))
            AND (@yearofrelease IS NULL OR m.year_of_release = @yearofrelease)
            GROUP BY id, userrating {orderClause}
            LIMIT @pageSize
            OFFSET @pageOffset
            """, new
        {
            userId = options.UserId,
            title = options.Title,
            yearofrelease = options.YearOfRelease,
            pageSize = options.PageSize,
            pageOffset = (options.Page - 1) * options.PageSize
        }, cancellationToken: token));

        return result.Select(m => new Movie
        {
            Id = m.id,
            Title = m.title,
            YearOfRelease = m.year_of_release,
            Rating = (float?)m.rating,
            UserRating = (int?)m.userrating,
            Genres = Enumerable.ToList(m.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("""
            DELETE FROM genres WHERE movieid = @id
            """, new { id = movie.Id }, cancellationToken: token));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO genres (movieid, name)
                VALUES (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE movies SET slug = @Slug, title = @title, year_of_release = @YearOfRelease
            WHERE id = @Id
            """, movie, cancellationToken: token));

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("""
            DELETE FROM genres WHERE movieId = @id
            """, new { id }, cancellationToken: token));

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            DELETE FROM movies WHERE id = @id
            """, new { id }, cancellationToken: token));

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
            SELECT COUNT(1) FROM movies WHERE id = @id
            """, new { id }, cancellationToken: token));
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.QuerySingleAsync<int>(new CommandDefinition("""
            SELECT COUNT(id) from movies
            WHERE (@title IS NULL OR title LIKE ('%' || @title || '%'))
            AND (@yearofrelease IS NULL OR year_of_release = @yearofrelease)
            """, new
        {
            title,
            yearOfRelease,
        }, cancellationToken: token));
    }
}