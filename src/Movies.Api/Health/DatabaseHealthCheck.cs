using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;

public class DatabaseHealthCheck(IDbConnectionFactory dbConnectionFacory, ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
    public const string Name = "Database";
    private readonly IDbConnectionFactory _dbConnectionFacory = dbConnectionFacory;
    private readonly ILogger<DatabaseHealthCheck> _logger = logger;
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await _dbConnectionFacory.CreateConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            const string errorMessage = "Database is unhealthy";
            _logger.LogError(e, errorMessage);
            return HealthCheckResult.Unhealthy(errorMessage);
        }
    }
}