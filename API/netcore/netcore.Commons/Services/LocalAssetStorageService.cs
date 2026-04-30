using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using netcore.Commons.Exceptions;
using netcore.Commons.Interfaces;
using netcore.Commons.Models;

namespace netcore.Commons.Services;

public sealed class LocalAssetStorageService : IObjectStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private readonly IWebHostEnvironment _env;
    private readonly LocalAssetStorageOptions _options;
    private readonly ILogger<LocalAssetStorageService> _logger;
    private readonly FileExtensionContentTypeProvider _contentTypes = new();

    public LocalAssetStorageService(
        IWebHostEnvironment env,
        IOptions<LocalAssetStorageOptions> options,
        ILogger<LocalAssetStorageService> logger)
    {
        _env = env;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string directory, CancellationToken ct = default)
    {
        ValidateImage(file);

        var root = GetStorageRoot();
        var safeDirectory = SanitizePathSegment(directory);
        var targetDirectory = Path.Combine(root, safeDirectory);
        Directory.CreateDirectory(targetDirectory);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}";
        var targetPath = Path.Combine(targetDirectory, fileName);

        await using (var stream = File.Create(targetPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var url = CombineUrl(_options.RequestPath, safeDirectory, fileName);
        _logger.LogInformation("Stored local asset {Url} at {Path}", url, targetPath);
        return url;
    }

    public async Task<ObjectStorageFileResult?> GetFileAsync(string fileUrl, CancellationToken ct = default)
    {
        var path = TryResolveManagedPath(fileUrl);
        if (path is null || !File.Exists(path))
        {
            return null;
        }

        _contentTypes.TryGetContentType(path, out var contentType);
        return new ObjectStorageFileResult
        {
            Content = await File.ReadAllBytesAsync(path, ct),
            ContentType = contentType ?? "application/octet-stream"
        };
    }

    public async Task<string?> ReplaceImageAsync(
        IFormFile? file,
        string? currentUrl,
        string directory,
        bool removeCurrent = false,
        string? fallbackUrl = null,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            if (removeCurrent)
            {
                await RemoveIfManagedAsync(currentUrl, ct);
                return string.IsNullOrWhiteSpace(fallbackUrl) ? null : fallbackUrl;
            }

            if (!string.IsNullOrWhiteSpace(fallbackUrl) &&
                !string.Equals(currentUrl, fallbackUrl, StringComparison.Ordinal))
            {
                await RemoveIfManagedAsync(currentUrl, ct);
                return fallbackUrl;
            }

            return currentUrl;
        }

        var newUrl = await UploadImageAsync(file, directory, ct);
        await RemoveIfManagedAsync(currentUrl, ct);
        return newUrl;
    }

    public Task RemoveIfManagedAsync(string? fileUrl, CancellationToken ct = default)
    {
        var path = TryResolveManagedPath(fileUrl);
        if (path is null || !File.Exists(path))
        {
            return Task.CompletedTask;
        }

        try
        {
            File.Delete(path);
            _logger.LogInformation("Deleted local asset {Path}", path);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Cannot delete local asset {Path}", path);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Cannot delete local asset {Path}", path);
        }

        return Task.CompletedTask;
    }

    private void ValidateImage(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new MessageException("Không tìm thấy file ảnh.");
        }

        if (file.Length > _options.MaxImageBytes)
        {
            throw new MessageException("Kích thước ảnh không được vượt quá 10MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new MessageException("Định dạng ảnh chưa được hỗ trợ.");
        }
    }

    private string GetStorageRoot()
    {
        var configuredRoot = string.IsNullOrWhiteSpace(_options.RootPath)
            ? "wwwroot/assets"
            : _options.RootPath;

        var root = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(_env.ContentRootPath, configuredRoot);

        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private string? TryResolveManagedPath(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return null;
        }

        var requestPath = NormalizeUrlPath(_options.RequestPath);
        var candidatePath = Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri)
            ? uri.AbsolutePath
            : fileUrl;

        candidatePath = NormalizeUrlPath(candidatePath);
        if (!candidatePath.StartsWith(requestPath + "/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var relativePath = candidatePath[requestPath.Length..].TrimStart('/');
        var root = GetStorageRoot();
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;
        var isInsideRoot = fullPath.Equals(root, StringComparison.OrdinalIgnoreCase)
            || fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase);

        return isInsideRoot ? fullPath : null;
    }

    private static string CombineUrl(params string[] parts)
        => "/" + string.Join("/", parts.Select(NormalizeUrlPath).Where(p => p.Length > 0));

    private static string NormalizeUrlPath(string value)
        => (value ?? string.Empty).Replace('\\', '/').Trim('/');

    private static string SanitizePathSegment(string value)
    {
        var normalized = NormalizeUrlPath(value);
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => string.Concat(segment.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.')))
            .Where(segment => !string.IsNullOrWhiteSpace(segment));

        return string.Join(Path.DirectorySeparatorChar, segments);
    }
}
