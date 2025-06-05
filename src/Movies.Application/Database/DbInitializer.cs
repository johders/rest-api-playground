using Dapper;

namespace Movies.Application.Database;

public class DbInitializer(IDbConnectionFactory dbConnectionFacory)
{
    private readonly IDbConnectionFactory _dbConnectionFacory = dbConnectionFacory;

    public async Task InitializeAsync()
    {
        using var connection = await _dbConnectionFacory.CreateConnectionAsync();

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS movies (
            id UUID PRIMARY KEY,
            slug TEXT NOT NULL,
            title TEXT NOT NULL,
            year_of_release INTEGER NOT NULL);
        """);

        await connection.ExecuteAsync("""
            CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS movies_slug_idx ON movies
            using btree(slug);
        """);

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS genres (
            movieid UUID REFERENCES movies (id),
            name TEXT NOT NULL);
        """);

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS ratings (
            userid UUID,
            movieid UUID REFERENCES movies (id),
            rating INTEGER NOT NULL,
            PRIMARY KEY (userid, movieid));
        """);
    }
}