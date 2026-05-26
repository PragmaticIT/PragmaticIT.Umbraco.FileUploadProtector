using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Scoping;

namespace PragmaticIT.Umbraco.FileUploadProtector;

internal sealed class MediaAuthorizationService : IMediaAuthorizationService
{
    private readonly IScopeProvider _scopeProvider;
    private readonly IMemberManager _memberManager;

    public MediaAuthorizationService(IScopeProvider scopeProvider, IMemberManager memberManager)
    {
        _scopeProvider = scopeProvider;
        _memberManager = memberManager;
    }

    public async Task<MediaAuthorizationResult> IsCurrentMemberAuthorizedAsync(string path, CancellationToken cancellationToken = default)
    {
        var docPath = GetRelatedDocPath(path);

        // File is not associated with any document – treat as public
        if (string.IsNullOrWhiteSpace(docPath))
            return MediaAuthorizationResult.NotFound;

        // File belongs to a document; check whether the current member may access it.
        // MemberHasAccessAsync returns true for documents that have no member restrictions,
        // so there is no need for a separate "is document restricted" check.
        return await _memberManager.MemberHasAccessAsync(docPath)
            ? MediaAuthorizationResult.AccessPermitted
            : MediaAuthorizationResult.AccessProhibited;
    }

    private const string Query = """
        SELECT un.path FROM
        umbracoNode un
        INNER JOIN umbracoDocument ud ON un.id = ud.nodeId AND ud.published = 1
        INNER JOIN umbracoContent uc ON un.id = uc.nodeId AND un.trashed = 0
        INNER JOIN umbracoContentVersion ucv ON uc.nodeId = ucv.nodeId AND ucv.[current] = 1
        INNER JOIN umbracoPropertyData upd ON ucv.id = upd.versionId
        WHERE upd.varcharValue = @0
        """;

    /// <summary>
    /// Searches for the published, non-trashed document whose property value matches <paramref name="path"/>.
    /// Returns <c>null</c> when no such document is found.
    /// </summary>
    private string? GetRelatedDocPath(string path)
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.ExecuteScalar<string>(Query, path);
    }
}
