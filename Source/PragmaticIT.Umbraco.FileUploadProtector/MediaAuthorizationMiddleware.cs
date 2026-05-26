using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;

namespace PragmaticIT.Umbraco.FileUploadProtector;

public sealed class MediaAuthorizationMiddleware(RequestDelegate next)
{
	private static readonly PathString MediaPathPrefix = new("/media");

	public async Task InvokeAsync(
		HttpContext context,
		IAuthenticationSchemeProvider schemeProvider,
		IMediaAuthorizationService mediaAuthorizationService)
	{
		if (!context.Request.Path.StartsWithSegments(MediaPathPrefix))
		{
			await next(context);
			return;
		}

		DisableCaching(context);

		var disableCache = true;
		context.Response.OnStarting(() =>
		{
			if (disableCache)
				DisableCaching(context);
			return Task.CompletedTask;
		});

		var backOfficeResult = await context.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
		if (backOfficeResult.Succeeded)
		{
			await next(context);
			return;
		}

		// Authenticate against all registered schemes so IMemberManager.IsLoggedIn()
		// works correctly regardless of how the member authenticated (cookie, OIDC, etc.).
		// Some schemes (e.g. OpenIddict) throw if called before UseAuthentication() –
		// skip those gracefully.
		var schemes = await schemeProvider.GetAllSchemesAsync();
		foreach (var scheme in schemes)
		{
			AuthenticateResult result;
			try
			{
				result = await context.AuthenticateAsync(scheme.Name);
			}
			catch (InvalidOperationException)
			{
				continue;
			}

			if (result.Succeeded && result.Principal is { Identity.IsAuthenticated: true })
			{
				context.User = result.Principal;
				break;
			}
		}

		var authorizationResult = await mediaAuthorizationService.IsCurrentMemberAuthorizedAsync(
			context.Request.Path,
			context.RequestAborted);

		if (authorizationResult == MediaAuthorizationResult.NotFound)
		{
			disableCache = false;
			await next(context);
			return;
		}

		if (authorizationResult == MediaAuthorizationResult.AccessPermitted)
		{
			await next(context);
			return;
		}

		context.Response.StatusCode = StatusCodes.Status401Unauthorized;
		await context.Response.WriteAsync("You are not allowed to download this file", context.RequestAborted);
	}

	private static void DisableCaching(HttpContext context)
	{
		context.Response.Headers.CacheControl = "no-cache, no-store";
		context.Response.Headers.Pragma = "no-cache";
		context.Response.Headers.Expires = "-1";
	}
}
