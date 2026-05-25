using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace netcore.Commons.Extensions;

/// <summary>
/// Thêm prefix /api/{service-name} vào tất cả controller routes.
/// Dùng trong Program.cs: options.Conventions.Add(new ApiPrefixRouteConvention("product"))
/// </summary>
public class ApiPrefixRouteConvention : IApplicationModelConvention
{
    private readonly string _servicePrefix;

    public ApiPrefixRouteConvention(string servicePrefix)
    {
        _servicePrefix = servicePrefix.ToLowerInvariant();
    }

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel is not null)
                {
                    selector.AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = $"api/{_servicePrefix}/" + selector.AttributeRouteModel.Template
                    };
                }
                else
                {
                    selector.AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = $"api/{_servicePrefix}"
                    };
                }
            }
        }
    }
}
