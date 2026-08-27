using System.Net;
using System.Text.RegularExpressions;
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

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/knowledgetech")]
    [InlineData("/Home/projectresult")]
    public async Task PageImagesAreServedFromLocalAssets(string path)
    {
        var response = await _client.GetAsync(path);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        var externalImageUrls = Regex.Matches(
                html,
                "<img\\b[^>]*\\bsrc=[\\\"'](?<url>https?://[^\\\"']+)[\\\"']",
                RegexOptions.IgnoreCase)
            .Select(match => match.Groups["url"].Value)
            .ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(externalImageUrls);
    }

    [Fact]
    public async Task EventPicturePageRendersEveryWorkshopPhotoFromLocalAssets()
    {
        string[] workshopPhotoNames =
        [
            "LINE_ALBUM_TDCworkshop1_260304_3.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_4.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_6.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_9.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_10.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_11.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_12.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_21.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_25.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_26.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_27.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_28.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_29.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_36.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_37.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_41.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_42.jpg",
            "LINE_ALBUM_TDCworkshop1_260304_47.jpg"
        ];

        var pageResponse = await _client.GetAsync("/Home/eventpic");
        var html = await pageResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);

        for (var index = 0; index < workshopPhotoNames.Length; index++)
        {
            var photoName = workshopPhotoNames[index];
            var photoPath = $"/asset/images/event/workshop1/{photoName}";

            Assert.Contains(
                $"src=\"{photoPath}\" alt=\"การประชุมเชิงปฏิบัติการ ครั้งที่ 1 ภาพที่ {index + 1}\"",
                html);
            Assert.Contains($"href=\"{photoPath}\"", html);

            var photoResponse = await _client.GetAsync(photoPath);
            Assert.Equal(HttpStatusCode.OK, photoResponse.StatusCode);
            Assert.Equal("image/jpeg", photoResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await photoResponse.Content.ReadAsByteArrayAsync());
        }
    }

    [Fact]
    public async Task EventPicturePageRendersEverySecondWorkshopPhotoFromLocalAssets()
    {
        string[] workshopPhotoNames =
        [
            "workshop2-01.jpg",
            "workshop2-02.jpg",
            "workshop2-03.jpg",
            "workshop2-04.jpg",
            "workshop2-05.jpg",
            "workshop2-06.jpg",
            "workshop2-07.jpg",
            "workshop2-08.jpg",
            "workshop2-09.jpg",
            "workshop2-10.jpg",
            "workshop2-11.jpg",
            "workshop2-12.jpg",
            "workshop2-13.jpg",
            "workshop2-14.jpg",
            "workshop2-15.jpg",
            "workshop2-16.jpg",
            "workshop2-17.jpg"
        ];

        var pageResponse = await _client.GetAsync("/Home/eventpic");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        Assert.Contains("การประชุมเชิงปฏิบัติการ ครั้งที่ 2", html);
        Assert.DoesNotContain("encrypted-tbn0.gstatic.com", html);

        for (var index = 0; index < workshopPhotoNames.Length; index++)
        {
            var photoName = workshopPhotoNames[index];
            var photoPath = $"/asset/images/event/workshop2/{photoName}";

            Assert.Contains(
                $"src=\"{photoPath}\" alt=\"การประชุมเชิงปฏิบัติการ ครั้งที่ 2 ภาพที่ {index + 1}\"",
                html);
            Assert.Contains($"href=\"{photoPath}\"", html);

            var photoResponse = await _client.GetAsync(photoPath);
            Assert.Equal(HttpStatusCode.OK, photoResponse.StatusCode);
            Assert.Equal("image/jpeg", photoResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await photoResponse.Content.ReadAsByteArrayAsync());
        }
    }

    [Fact]
    public async Task InformationPageRendersSeventeenthIssueFromLocalAssets()
    {
        string[] issuePageNames =
        [
            "1.jpg",
            "2.jpg",
            "3.jpg",
            "4.jpg",
            "5.jpg"
        ];

        var pageResponse = await _client.GetAsync("/Home/information");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        Assert.Contains("ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 17", html);
        Assert.Contains("href=\"#m17-modal\"", html);
        Assert.Contains("id=\"m17-modal\"", html);
        Assert.Contains("aria-label=\"ปิดข่าวประชาสัมพันธ์โครงการ ฉบับที่ 17\"", html);
        Assert.Contains(
            "src=\"/asset/images/event/m17/1.jpg\" alt=\"ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 17\" style=\"width:100%;height:250px;\"",
            html);

        for (var index = 0; index < issuePageNames.Length; index++)
        {
            var pageName = issuePageNames[index];
            var pagePath = $"/asset/images/event/m17/{pageName}";

            Assert.Contains(
                $"src=\"{pagePath}\" alt=\"ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 17 หน้า {index + 1}\"",
                html);

            var imageResponse = await _client.GetAsync(pagePath);
            Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
            Assert.Equal("image/jpeg", imageResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await imageResponse.Content.ReadAsByteArrayAsync());
        }
    }

    [Fact]
    public async Task InformationPageRendersEighteenthIssueFromLocalAssets()
    {
        string[] issuePageNames =
        [
            "1.jpg",
            "2.jpg"
        ];

        var pageResponse = await _client.GetAsync("/Home/information");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        Assert.Contains("ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 18", html);
        Assert.Contains("href=\"#m18-modal\"", html);
        Assert.Contains("id=\"m18-modal\"", html);

        foreach (var pageName in issuePageNames)
        {
            var pagePath = $"/asset/images/event/m18/{pageName}";

            Assert.Contains($"src=\"{pagePath}\"", html);

            var imageResponse = await _client.GetAsync(pagePath);
            Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
            Assert.Equal("image/jpeg", imageResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await imageResponse.Content.ReadAsByteArrayAsync());
        }
    }

    [Fact]
    public async Task InformationPageRendersNineteenthIssueFromLocalAssets()
    {
        string[] issuePageNames =
        [
            "1.png",
            "2.png",
            "3.png"
        ];

        var pageResponse = await _client.GetAsync("/Home/information");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        Assert.Contains("ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 19", html);
        Assert.Contains("href=\"#m19-modal\"", html);
        Assert.Contains("id=\"m19-modal\"", html);

        foreach (var pageName in issuePageNames)
        {
            var pagePath = $"/asset/images/event/m19/{pageName}";

            Assert.Contains($"src=\"{pagePath}\"", html);

            var imageResponse = await _client.GetAsync(pagePath);
            Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
            Assert.Equal("image/png", imageResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await imageResponse.Content.ReadAsByteArrayAsync());
        }
    }

    [Fact]
    public async Task InformationPageRendersTwentiethIssueFromLocalAssets()
    {
        string[] issuePageNames =
        [
            "1.png",
            "2.png",
            "3.png",
            "4.png"
        ];

        var pageResponse = await _client.GetAsync("/Home/information");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        Assert.Contains("ข่าวประชาสัมพันธ์โครงการ ฉบับที่ 20", html);
        Assert.Contains("href=\"#m20-modal\"", html);
        Assert.Contains("id=\"m20-modal\"", html);

        foreach (var pageName in issuePageNames)
        {
            var pagePath = $"/asset/images/event/m20/{pageName}";

            Assert.Contains($"src=\"{pagePath}\"", html);

            var imageResponse = await _client.GetAsync(pagePath);
            Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
            Assert.Equal("image/png", imageResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await imageResponse.Content.ReadAsByteArrayAsync());
        }
    }
}
