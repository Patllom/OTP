using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace OTP.Web.Tests;

public sealed class HomeControllerContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HomeControllerContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public static TheoryData<string> LegacyActions => new()
    {
        "Index",
        "objective",
        "scopeofwork",
        "eventpic",
        "knowledgetech",
        "information",
        "projectresult",
        "About",
        "Contact"
    };

    [Theory]
    [MemberData(nameof(LegacyActions))]
    public void LegacyActionPreservesMvcViewContract(string actionName)
    {
        using var scope = _factory.Services.CreateScope();
        var descriptorProvider = scope.ServiceProvider
            .GetRequiredService<IActionDescriptorCollectionProvider>();

        var descriptor = Assert.Single(
            descriptorProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>(),
            item => item.ControllerName == "Home" && item.ActionName == actionName);

        var controller = Activator.CreateInstance(descriptor.ControllerTypeInfo.AsType());
        Assert.NotNull(controller);

        var result = descriptor.MethodInfo.Invoke(controller, parameters: null);
        Assert.IsType<ViewResult>(result);
    }
}
