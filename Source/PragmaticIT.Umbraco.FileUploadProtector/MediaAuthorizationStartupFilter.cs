using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace PragmaticIT.Umbraco.FileUploadProtector;

internal sealed class MediaAuthorizationStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        => app =>
        {
            app.UseMiddleware<MediaAuthorizationMiddleware>();
            next(app);
        };
}
