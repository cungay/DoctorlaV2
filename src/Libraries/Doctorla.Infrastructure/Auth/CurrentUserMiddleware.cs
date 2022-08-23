using Microsoft.AspNetCore.Http;

namespace Doctorla.Infrastructure.Auth;

public class CurrentUserMiddleware : IMiddleware
{
    private readonly ICurrentUserInitializer currentUserInitializer = null;

    public CurrentUserMiddleware(ICurrentUserInitializer currentUserInitializer) =>
        this.currentUserInitializer = currentUserInitializer;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        this.currentUserInitializer.SetCurrentUser(context.User);

        await next(context);
    }
}