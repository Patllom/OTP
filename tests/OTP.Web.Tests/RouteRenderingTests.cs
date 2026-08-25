using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OTP.Web.Tests;

public sealed class RouteRenderingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RouteRenderingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public static TheoryData<string, string> RenderedPages => new()
    {
        { "/", "วัตถุประสงค์โครงการ" },
        { "/Home/Index", "วัตถุประสงค์โครงการ" },
        { "/Home/objective", "วัตถุประสงค์" },
        { "/Home/scopeofwork", "content1.png" },
        { "/Home/eventpic", "ประมวลภาพ" },
        { "/Home/knowledgetech", "การถ่ายทอดองค์ความรู้และเทคโนโลยี" },
        { "/Home/information", "ข่าวประชาสัมพันธ์" },
        { "/Home/projectresult", "ผลการดำเนินงาน" },
        { "/Home/About", "ความเป็นมาของโครงการ" },
        { "/Home/Contact", "ติดต่อโครงการ" },
        { "/Home/Error", "An error occurred while processing your request." }
    };

    [Theory]
    [MemberData(nameof(RenderedPages))]
    public async Task PageRouteRendersExpectedHtml(string path, string marker)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(marker, html);
    }

    [Fact]
    public async Task ProjectLogoIsServedFromItsLegacyUrl()
    {
        var response = await _client.GetAsync("/asset/images/content/logo1.png");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.NotEmpty(await response.Content.ReadAsByteArrayAsync());
    }
}
