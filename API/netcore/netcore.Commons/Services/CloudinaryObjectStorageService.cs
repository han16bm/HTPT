using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using netcore.Commons.Exceptions;
using netcore.Commons.Interfaces;
using netcore.Commons.Models;

namespace netcore.Commons.Services;

public class CloudinaryObjectStorageService : IObjectStorageService
{
    private static readonly Dictionary<string, byte[][]> ImageSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        [".jpeg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        [".png"] = [new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }],
        [".gif"] = [Encoding.ASCII.GetBytes("GIF87a"), Encoding.ASCII.GetBytes("GIF89a")],
        [".webp"] = [Encoding.ASCII.GetBytes("RIFF")],
        [".bmp"] = [Encoding.ASCII.GetBytes("BM")]
    };

    private const long MaxImageBytes = 10 * 1024 * 1024; // 10MB

    private readonly CloudinaryOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CloudinaryObjectStorageService> _logger;

    public CloudinaryObjectStorageService(
        IOptions<CloudinaryOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<CloudinaryObjectStorageService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        ApplyCloudinaryUrlIfNeeded(_options);

        if (string.IsNullOrWhiteSpace(_options.CloudName) ||
            string.IsNullOrWhiteSpace(_options.ApiKey) ||
            string.IsNullOrWhiteSpace(_options.ApiSecret))
        {
            throw new InvalidOperationException("Cloudinary configuration is missing or incomplete.");
        }
    }

    public async Task<string> UploadImageAsync(IFormFile file, string directory, CancellationToken ct = default)
    {
        if (file is null)
        {
            throw new MessageException("Không tìm thấy file ảnh.");
        }

        var payload = await ReadAndValidateImageAsync(file, ct);
        var publicId = BuildPublicId(directory);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var mimeType = GetMimeType(extension);

        var parametersToSign = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["public_id"] = publicId,
            ["timestamp"] = timestamp
        };

        if (!string.IsNullOrWhiteSpace(_options.UploadPreset))
        {
            parametersToSign["upload_preset"] = _options.UploadPreset;
        }

        var signature = Sign(parametersToSign);

        // Gửi file dưới dạng binary stream (không phải base64) để tránh lỗi encoding
        var fileContent = new ByteArrayContent(payload);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

        using var content = new MultipartFormDataContent
        {
            { new StringContent(_options.ApiKey), "api_key" },
            { fileContent, "file", Path.GetFileName(file.FileName) },
            { new StringContent(publicId), "public_id" },
            { new StringContent(signature), "signature" },
            { new StringContent(timestamp), "timestamp" }
        };

        if (!string.IsNullOrWhiteSpace(_options.UploadPreset))
        {
            content.Add(new StringContent(_options.UploadPreset), "upload_preset");
        }

        // Basic Auth: api_key:api_secret (Cloudinary signed upload yêu cầu header này)
        var authToken = Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{_options.ApiKey}:{_options.ApiSecret}"));

        var client = _httpClientFactory.CreateClient(nameof(CloudinaryObjectStorageService));

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUploadEndpoint())
        {
            Content = content
        };
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

        _logger.LogInformation(
            "Uploading image to Cloudinary. CloudName={CloudName}, PublicId={PublicId}, FileName={FileName}, Extension={Extension}, Size={Size}",
            _options.CloudName,
            publicId,
            file.FileName,
            extension,
            file.Length);

        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Cloudinary upload failed. StatusCode={StatusCode}, PublicId={PublicId}, Response={Response}",
                    (int)response.StatusCode,
                    publicId,
                    body);

                throw new MessageException($"Upload file lên Cloudinary thất bại ({(int)response.StatusCode}): {body}");
            }

            using var json = JsonDocument.Parse(body);

            if (!json.RootElement.TryGetProperty("secure_url", out var secureUrlElement))
            {
                _logger.LogError(
                    "Cloudinary upload succeeded but secure_url is missing. PublicId={PublicId}, Response={Response}",
                    publicId,
                    body);

                throw new MessageException("Cloudinary không trả về secure_url sau khi upload.");
            }

            var secureUrl = secureUrlElement.GetString();
            if (string.IsNullOrWhiteSpace(secureUrl))
            {
                throw new MessageException("secure_url trả về từ Cloudinary không hợp lệ.");
            }

            _logger.LogInformation(
                "Uploaded image to Cloudinary successfully. PublicId={PublicId}, SecureUrl={SecureUrl}",
                publicId,
                secureUrl);

            return secureUrl;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout khi upload ảnh lên Cloudinary. PublicId={PublicId}", publicId);
            throw new MessageException("Hết thời gian chờ khi upload ảnh lên Cloudinary.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Lỗi kết nối khi upload ảnh lên Cloudinary. PublicId={PublicId}", publicId);
            throw new MessageException("Không thể kết nối tới Cloudinary.");
        }
    }

    public async Task<ObjectStorageFileResult?> GetFileAsync(string fileUrl, CancellationToken ct = default)
    {
        if (!IsManagedUrl(fileUrl))
        {
            return null;
        }

        var client = _httpClientFactory.CreateClient(nameof(CloudinaryObjectStorageService));

        try
        {
            using var response = await client.GetAsync(fileUrl, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "Read file from Cloudinary failed. Url={Url}, StatusCode={StatusCode}, Response={Response}",
                    fileUrl,
                    (int)response.StatusCode,
                    error);

                throw new MessageException($"Đọc file từ Cloudinary thất bại ({(int)response.StatusCode}): {error}");
            }

            return new ObjectStorageFileResult
            {
                Content = await response.Content.ReadAsByteArrayAsync(ct),
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream"
            };
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout khi đọc file từ Cloudinary. Url={Url}", fileUrl);
            throw new MessageException("Hết thời gian chờ khi đọc file từ Cloudinary.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Lỗi kết nối khi đọc file từ Cloudinary. Url={Url}", fileUrl);
            throw new MessageException("Không thể kết nối tới Cloudinary.");
        }
    }

    public async Task<string?> ReplaceImageAsync(
        IFormFile? file,
        string? currentUrl,
        string directory,
        bool removeCurrent = false,
        string? fallbackUrl = null,
        CancellationToken ct = default)
    {
        if (removeCurrent)
        {
            await RemoveIfManagedAsync(currentUrl, ct);
            currentUrl = null;
        }

        if (file is not null)
        {
            var newUrl = await UploadImageAsync(file, directory, ct);

            if (!string.IsNullOrWhiteSpace(currentUrl) &&
                !string.Equals(currentUrl, newUrl, StringComparison.OrdinalIgnoreCase))
            {
                await RemoveIfManagedAsync(currentUrl, ct);
            }

            return newUrl;
        }

        if (!string.IsNullOrWhiteSpace(fallbackUrl))
        {
            var normalized = fallbackUrl.Trim();

            if (!string.IsNullOrWhiteSpace(currentUrl) &&
                !string.Equals(currentUrl, normalized, StringComparison.OrdinalIgnoreCase))
            {
                await RemoveIfManagedAsync(currentUrl, ct);
            }

            return normalized;
        }

        return currentUrl;
    }

    public async Task RemoveIfManagedAsync(string? fileUrl, CancellationToken ct = default)
    {
        var publicId = TryExtractPublicId(fileUrl);
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var parametersToSign = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["invalidate"] = "true",
            ["public_id"] = publicId,
            ["timestamp"] = timestamp
        };

        var signature = Sign(parametersToSign);

        using var content = new MultipartFormDataContent
        {
            { new StringContent(_options.ApiKey), "api_key" },
            { new StringContent("true"), "invalidate" },
            { new StringContent(publicId), "public_id" },
            { new StringContent(signature), "signature" },
            { new StringContent(timestamp), "timestamp" }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildDestroyEndpoint())
        {
            Content = content
        };

        var client = _httpClientFactory.CreateClient(nameof(CloudinaryObjectStorageService));

        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Delete file on Cloudinary failed. PublicId={PublicId}, StatusCode={StatusCode}, Response={Response}",
                    publicId,
                    (int)response.StatusCode,
                    body);
                return;
            }

            using var json = JsonDocument.Parse(body);

            if (json.RootElement.TryGetProperty("result", out var resultElement))
            {
                var result = resultElement.GetString();
                if (string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(result, "not found", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "Deleted Cloudinary resource. PublicId={PublicId}, Result={Result}",
                        publicId,
                        result);
                    return;
                }
            }

            _logger.LogWarning(
                "Unexpected Cloudinary delete response. PublicId={PublicId}, Response={Response}",
                publicId,
                body);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Timeout khi xóa file trên Cloudinary. PublicId={PublicId}", publicId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Lỗi kết nối khi xóa file trên Cloudinary. PublicId={PublicId}", publicId);
        }
    }

    private async Task<byte[]> ReadAndValidateImageAsync(IFormFile file, CancellationToken ct)
    {
        if (file.Length <= 0)
        {
            throw new MessageException("File ảnh không hợp lệ.");
        }

        if (file.Length > MaxImageBytes)
        {
            throw new MessageException("Kích thước ảnh không được vượt quá 10MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !ImageSignatures.ContainsKey(extension))
        {
            throw new MessageException("Định dạng ảnh chưa được hỗ trợ.");
        }

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        if (!MatchesImageSignature(bytes, extension))
        {
            throw new MessageException("Nội dung file ảnh không hợp lệ.");
        }

        if (string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase))
        {
            if (bytes.Length < 12 ||
                !Encoding.ASCII.GetString(bytes, 8, 4).Equals("WEBP", StringComparison.Ordinal))
            {
                throw new MessageException("Nội dung file WebP không hợp lệ.");
            }
        }

        return bytes;
    }

    private bool MatchesImageSignature(byte[] bytes, string extension)
    {
        foreach (var signature in ImageSignatures[extension])
        {
            if (bytes.Length < signature.Length)
            {
                continue;
            }

            var matched = true;
            for (var i = 0; i < signature.Length; i++)
            {
                if (bytes[i] != signature[i])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return true;
            }
        }

        return false;
    }

    private static void ApplyCloudinaryUrlIfNeeded(CloudinaryOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.CloudinaryUrl))
        {
            return;
        }

        if (!Uri.TryCreate(options.CloudinaryUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, "cloudinary", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("CloudinaryUrl is invalid.");
        }

        var userInfoParts = uri.UserInfo.Split(':', 2);
        if (userInfoParts.Length != 2)
        {
            throw new InvalidOperationException("CloudinaryUrl must include api_key and api_secret.");
        }

        options.CloudName = uri.Host;
        options.ApiKey = Uri.UnescapeDataString(userInfoParts[0]);
        options.ApiSecret = Uri.UnescapeDataString(userInfoParts[1]);
    }

    private string BuildPublicId(string directory)
    {
        var normalizedDirectory = string.Join('/',
            (directory ?? string.Empty)
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(SanitizePathSegment)
                .Where(x => !string.IsNullOrWhiteSpace(x)));

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";

        return string.IsNullOrWhiteSpace(normalizedDirectory)
            ? fileName
            : $"{normalizedDirectory}/{fileName}";
    }

    private string BuildUploadEndpoint()
        => $"https://api.cloudinary.com/v1_1/{_options.CloudName}/image/upload";

    private string BuildDestroyEndpoint()
        => $"https://api.cloudinary.com/v1_1/{_options.CloudName}/image/destroy";

    private string Sign(SortedDictionary<string, string> parameters)
    {
        var payload = string.Join("&",
            parameters
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(kvp => $"{kvp.Key}={kvp.Value}"));

        var raw = $"{payload}{_options.ApiSecret}";
        var bytes = Encoding.UTF8.GetBytes(raw);
        var algorithm = _options.SignatureAlgorithm?.Trim().ToLowerInvariant();
        var hash = algorithm switch
        {
            null or "" or "sha1" => SHA1.HashData(bytes),
            "sha256" => SHA256.HashData(bytes),
            _ => throw new InvalidOperationException(
                $"Cloudinary signature algorithm '{_options.SignatureAlgorithm}' is not supported.")
        };

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GetMimeType(string extension) => extension switch
    {
        ".jpg" => "image/jpeg",
        ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".bmp" => "image/bmp",
        _ => "application/octet-stream"
    };

    private bool IsManagedUrl(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) ||
            !Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Host.Contains("cloudinary.com", StringComparison.OrdinalIgnoreCase)
            && uri.AbsolutePath.Contains($"/{_options.CloudName}/", StringComparison.OrdinalIgnoreCase);
    }

    private string? TryExtractPublicId(string? fileUrl)
    {
        if (!IsManagedUrl(fileUrl) || !Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var uploadMarker = "/upload/";
        var absolutePath = Uri.UnescapeDataString(uri.AbsolutePath);
        var markerIndex = absolutePath.IndexOf(uploadMarker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        var publicPart = absolutePath[(markerIndex + uploadMarker.Length)..].Trim('/');
        if (string.IsNullOrWhiteSpace(publicPart))
        {
            return null;
        }

        var segments = publicPart.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return null;
        }

        var startIndex = segments[0].StartsWith("v", StringComparison.OrdinalIgnoreCase) &&
                         segments[0].Length > 1 &&
                         segments[0].Skip(1).All(char.IsDigit)
            ? 1
            : 0;

        if (startIndex >= segments.Length)
        {
            return null;
        }

        var relevantSegments = segments[startIndex..].ToArray();
        var lastSegment = relevantSegments[^1];
        var extension = Path.GetExtension(lastSegment);

        if (!string.IsNullOrWhiteSpace(extension))
        {
            relevantSegments[^1] = lastSegment[..^extension.Length];
        }

        return string.Join('/', relevantSegments);
    }

    private static string SanitizePathSegment(string value)
    {
        var sanitized = new string(
            value.Trim()
                .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? char.ToLowerInvariant(ch) : '-')
                .ToArray());

        while (sanitized.Contains("--", StringComparison.Ordinal))
        {
            sanitized = sanitized.Replace("--", "-", StringComparison.Ordinal);
        }

        sanitized = sanitized.Trim('-');

        return string.IsNullOrWhiteSpace(sanitized) ? "item" : sanitized;
    }
}
