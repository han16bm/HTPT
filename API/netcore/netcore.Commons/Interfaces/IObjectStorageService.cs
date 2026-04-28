using Microsoft.AspNetCore.Http;
using netcore.Commons.Models;

namespace netcore.Commons.Interfaces;

public interface IObjectStorageService
{
    Task<string> UploadImageAsync(IFormFile file, string directory, CancellationToken ct = default);
    Task<ObjectStorageFileResult?> GetFileAsync(string fileUrl, CancellationToken ct = default);
    Task<string?> ReplaceImageAsync(
        IFormFile? file,
        string? currentUrl,
        string directory,
        bool removeCurrent = false,
        string? fallbackUrl = null,
        CancellationToken ct = default);
    Task RemoveIfManagedAsync(string? fileUrl, CancellationToken ct = default);
}
