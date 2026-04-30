using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Controllers;
using netcore.Commons.Interfaces;

namespace API.Product.Controllers;

[Route("product-images")]
public class ProductImagesController : BaseApiController
{
    private readonly IObjectStorageService _storage;

    public ProductImagesController(IObjectStorageService storage)
    {
        _storage = storage;
    }

    // GET /api/product/product-images?url=https://...
    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> XemAnh([FromQuery] string url, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest("Thiếu URL ảnh.");
        }

        var file = await _storage.GetFileAsync(url, ct);
        if (file is null)
        {
            return NotFound();
        }

        return File(file.Content, file.ContentType);
    }
}
