using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace PragmaticIT.Umbraco.FileUploadProtector;

public sealed class FileUploadProtectorComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<IMediaAuthorizationService, MediaAuthorizationService>();
        builder.Services.AddSingleton<IStartupFilter, MediaAuthorizationStartupFilter>();
    }
}
