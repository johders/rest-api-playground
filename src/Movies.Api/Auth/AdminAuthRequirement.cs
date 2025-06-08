using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Movies.Api.Auth;

public class AdminAuthRequirement(string apiKey) : IAuthorizationHandler, IAuthorizationRequirement
{
    private readonly string _apiKey = apiKey;
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
        {
            context.Succeed(this);
            return Task.CompletedTask;
        }

        var httpContext = context.Resource as HttpContext;
        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        if (!httpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (_apiKey != extractedApiKey)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var identity = (ClaimsIdentity)httpContext.User.Identity!;

        identity.AddClaim(new Claim("userid", Guid.Parse("00000000-0000-0000-0000-111111111111").ToString()));
        context.Succeed(this);
        return Task.CompletedTask;
    }
}