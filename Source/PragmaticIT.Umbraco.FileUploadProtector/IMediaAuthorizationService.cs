namespace PragmaticIT.Umbraco.FileUploadProtector;

public interface IMediaAuthorizationService
{
    /// <summary>
    /// Checks whether the current member is authorized to access the media at the given path.
    /// </summary>
    Task<MediaAuthorizationResult> IsCurrentMemberAuthorizedAsync(string path, CancellationToken cancellationToken = default);
}
