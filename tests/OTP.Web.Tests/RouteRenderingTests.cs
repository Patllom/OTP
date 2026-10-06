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

    public static TheoryData<string, string[]> LocalPageImageAssets => new()
    {
        {
            "/",
            [
                "/asset/images/production/home-objective-1.png",
                "/asset/images/production/home-objective-2.jpg",
                "/asset/images/production/home-objective-3.jpg"
            ]
        },
        {
            "/Home/knowledgetech",
            []
        },
        {
            "/Home/projectresult",
            ["/asset/images/production/project-result.png"]
        }
    };

    public static TheoryData<string> ProductionImageAssets => new()
    {
        "/asset/images/production/home-objective-1.png",
        "/asset/images/production/home-objective-2.jpg",
        "/asset/images/production/home-objective-3.jpg",
        "/asset/images/production/home-news-1.png",
        "/asset/images/production/home-news-2.png",
        "/asset/images/production/home-news-3.jpg",
        "/asset/images/production/home-news-4.jpg",
        "/asset/images/production/home-news-5.jpg",
        "/asset/images/production/home-news-6.jpg",
        "/asset/images/production/knowledge-overview.png",
        "/asset/images/production/project-result.png"
    };

    [Theory]
    [MemberData(nameof(LocalPageImageAssets))]
    public async Task PageImagesAreServedFromLocalAssets(string path, string[] expectedImageUrls)
    {
        var response = await _client.GetAsync(path);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        var imageUrls = Regex.Matches(
                html,
                "<img\\b[^>]*\\bsrc=[\\\"'](?<url>[^\\\"']+)[\\\"']",
                RegexOptions.IgnoreCase)
            .Select(match => match.Groups["url"].Value)
            .Select(url => url.StartsWith("~/", StringComparison.Ordinal) ? url[1..] : url)
            .ToArray();
        var externalImageUrls = imageUrls
            .Where(url => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(externalImageUrls);

        foreach (var imageUrl in expectedImageUrls)
        {
            Assert.Contains(imageUrl, imageUrls);

            var imageResponse = await _client.GetAsync(imageUrl);
            Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
            Assert.Equal(ExpectedImageMediaType(imageUrl), imageResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await imageResponse.Content.ReadAsByteArrayAsync());
        }
    }

    [Theory]
    [MemberData(nameof(ProductionImageAssets))]
    public async Task ProductionImageAssetIsServed(string imageUrl)
    {
        var imageResponse = await _client.GetAsync(imageUrl);

        Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
        Assert.Equal(ExpectedImageMediaType(imageUrl), imageResponse.Content.Headers.ContentType?.MediaType);
        Assert.NotEmpty(await imageResponse.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task KnowledgeTechnologyPageRendersSkillsDevelopmentGalleriesInOrder()
    {
        var galleryPhotoCounts = new[]
        {
            (GalleryNumber: 2, PhotoCount: 13),
            (GalleryNumber: 3, PhotoCount: 14),
            (GalleryNumber: 4, PhotoCount: 15),
            (GalleryNumber: 5, PhotoCount: 13)
        };

        var pageResponse = await _client.GetAsync("/Home/knowledgetech");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);

        var previousHeadingIndex = html.IndexOf(
            "การพัฒนาทักษะความรู้ ครั้งที่ 1",
            StringComparison.Ordinal);
        Assert.True(previousHeadingIndex >= 0);

        foreach (var gallery in galleryPhotoCounts)
        {
            var heading = $"การพัฒนาทักษะความรู้ ครั้งที่ {gallery.GalleryNumber}";
            var headingIndex = html.IndexOf(heading, StringComparison.Ordinal);

            Assert.True(
                headingIndex > previousHeadingIndex,
                $"Expected {heading} to appear after the preceding gallery.");
            previousHeadingIndex = headingIndex;

            for (var photoNumber = 1; photoNumber <= gallery.PhotoCount; photoNumber++)
            {
                var photoPath = $"/asset/images/knowledgetech/sub{gallery.GalleryNumber}/{photoNumber}.jpg";
                var altText = $"{heading} ภาพที่ {photoNumber}";

                Assert.Contains(
                    $"href=\"{photoPath}\" aria-label=\"เปิด{altText}\"",
                    html);
                Assert.Contains(
                    $"src=\"{photoPath}\" alt=\"{altText}\" loading=\"lazy\"",
                    html);

                var photoResponse = await _client.GetAsync(photoPath);
                Assert.Equal(HttpStatusCode.OK, photoResponse.StatusCode);
                Assert.Equal("image/jpeg", photoResponse.Content.Headers.ContentType?.MediaType);
                Assert.NotEmpty(await photoResponse.Content.ReadAsByteArrayAsync());
            }
        }
    }

    private static string ExpectedImageMediaType(string imageUrl) =>
        Path.GetExtension(imageUrl).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => throw new ArgumentOutOfRangeException(nameof(imageUrl), imageUrl, "Unsupported image extension")
        };

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
    public async Task EventPicturePageRendersThirdWorkshopBeforeTrainingGalleries()
    {
        string[] workshopPhotoNames =
        [
            "workshop3-01.jpg",
            "workshop3-02.jpg",
            "workshop3-03.jpg",
            "workshop3-04.jpg",
            "workshop3-05.jpg",
            "workshop3-06.jpg",
            "workshop3-07.jpg",
            "workshop3-08.jpg",
            "workshop3-09.jpg",
            "workshop3-10.jpg",
            "workshop3-11.jpg",
            "workshop3-12.jpg",
            "workshop3-13.jpg",
            "workshop3-14.jpg",
            "workshop3-15.jpg",
            "workshop3-16.jpg",
            "workshop3-17.jpg"
        ];

        var pageResponse = await _client.GetAsync("/Home/eventpic");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);

        var workshop2HeadingIndex = html.IndexOf("การประชุมเชิงปฏิบัติการ ครั้งที่ 2", StringComparison.Ordinal);
        var workshop3HeadingIndex = html.IndexOf("การประชุมเชิงปฏิบัติการ ครั้งที่ 3", StringComparison.Ordinal);
        var firstTrainingHeadingIndex = html.IndexOf("การฝึกอบรม ครั้งที่ 1", StringComparison.Ordinal);
        var secondTrainingHeadingIndex = html.IndexOf("การฝึกอบรม ครั้งที่ 2", StringComparison.Ordinal);

        Assert.True(workshop2HeadingIndex >= 0);
        Assert.True(workshop3HeadingIndex > workshop2HeadingIndex);
        Assert.True(firstTrainingHeadingIndex > workshop3HeadingIndex);
        Assert.True(secondTrainingHeadingIndex > firstTrainingHeadingIndex);

        for (var index = 0; index < workshopPhotoNames.Length; index++)
        {
            var photoName = workshopPhotoNames[index];
            var photoPath = $"/asset/images/event/workshop3/{photoName}";

            Assert.Contains(
                $"src=\"{photoPath}\" alt=\"การประชุมเชิงปฏิบัติการ ครั้งที่ 3 ภาพที่ {index + 1}\"",
                html);
            Assert.Contains($"href=\"{photoPath}\"", html);

            var photoResponse = await _client.GetAsync(photoPath);
            Assert.Equal(HttpStatusCode.OK, photoResponse.StatusCode);
            Assert.Equal("image/jpeg", photoResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await photoResponse.Content.ReadAsByteArrayAsync());
        }
    }

    [Fact]
    public async Task EventPicturePageRendersTrainingGalleriesAfterSecondWorkshop()
    {
        string[] firstTrainingPhotoNames =
        [
            "training1-01.jpg",
            "training1-02.jpg",
            "training1-03.jpg",
            "training1-04.jpg",
            "training1-05.jpg",
            "training1-06.jpg",
            "training1-07.jpg",
            "training1-08.jpg",
            "training1-09.jpg",
            "training1-10.jpg",
            "training1-11.jpg",
            "training1-12.jpg",
            "training1-13.jpg",
            "training1-14.jpg",
            "training1-15.jpg",
            "training1-16.jpg",
            "training1-17.jpg",
            "training1-18.jpg",
            "training1-19.jpg",
            "training1-20.jpg",
            "training1-21.jpg",
            "training1-22.jpg",
            "training1-23.jpg",
            "training1-24.jpg",
            "training1-25.jpg",
            "training1-26.jpg"
        ];
        string[] secondTrainingPhotoNames =
        [
            "training2-01.jpg",
            "training2-02.jpg",
            "training2-03.jpg",
            "training2-04.jpg",
            "training2-05.jpg",
            "training2-06.jpg",
            "training2-07.jpg",
            "training2-08.jpg",
            "training2-09.jpg",
            "training2-10.jpg",
            "training2-11.jpg",
            "training2-12.jpg",
            "training2-13.jpg",
            "training2-14.jpg",
            "training2-15.jpg",
            "training2-16.jpg",
            "training2-17.jpg",
            "training2-18.jpg"
        ];

        var pageResponse = await _client.GetAsync("/Home/eventpic");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);

        var workshop2HeadingIndex = html.IndexOf("การประชุมเชิงปฏิบัติการ ครั้งที่ 2", StringComparison.Ordinal);
        var firstTrainingHeadingIndex = html.IndexOf("การฝึกอบรม ครั้งที่ 1", StringComparison.Ordinal);
        var secondTrainingHeadingIndex = html.IndexOf("การฝึกอบรม ครั้งที่ 2", StringComparison.Ordinal);

        Assert.True(workshop2HeadingIndex >= 0);
        Assert.True(firstTrainingHeadingIndex > workshop2HeadingIndex);
        Assert.True(secondTrainingHeadingIndex > firstTrainingHeadingIndex);

        var galleries = new[]
        {
            (Heading: "การฝึกอบรม ครั้งที่ 1", Folder: "training1", PhotoNames: firstTrainingPhotoNames),
            (Heading: "การฝึกอบรม ครั้งที่ 2", Folder: "training2", PhotoNames: secondTrainingPhotoNames)
        };

        foreach (var gallery in galleries)
        {
            for (var index = 0; index < gallery.PhotoNames.Length; index++)
            {
                var photoPath = $"/asset/images/event/{gallery.Folder}/{gallery.PhotoNames[index]}";

                Assert.Contains(
                    $"src=\"{photoPath}\" alt=\"{gallery.Heading} ภาพที่ {index + 1}\"",
                    html);
                Assert.Contains($"href=\"{photoPath}\"", html);

                var photoResponse = await _client.GetAsync(photoPath);
                Assert.Equal(HttpStatusCode.OK, photoResponse.StatusCode);
                Assert.Equal("image/jpeg", photoResponse.Content.Headers.ContentType?.MediaType);
                Assert.NotEmpty(await photoResponse.Content.ReadAsByteArrayAsync());
            }
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

    public static TheoryData<int, string[]> NewProjectNewsIssues => new()
    {
        { 21, ["1.jpg", "2.jpg"] },
        { 22, ["1.jpg", "2.jpg"] },
        { 23, ["1.jpg", "2.jpg", "3.jpg"] }
    };

    [Theory]
    [MemberData(nameof(NewProjectNewsIssues))]
    public async Task InformationPageRendersNewProjectNewsIssueFromLocalAssets(
        int issueNumber,
        string[] issuePageNames)
    {
        var pageResponse = await _client.GetAsync("/Home/information");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());
        var issueTitle = $"ข่าวประชาสัมพันธ์โครงการ ฉบับที่ {issueNumber}";

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        Assert.Contains(issueTitle, html);
        Assert.Contains($"href=\"#m{issueNumber}-modal\"", html);
        Assert.Contains($"id=\"m{issueNumber}-modal\"", html);
        Assert.Contains($"aria-label=\"ปิด{issueTitle}\"", html);
        Assert.Contains($"#m{issueNumber}-modal:target", html);
        Assert.Contains(
            $"src=\"/asset/images/event/m{issueNumber}/1.jpg\" alt=\"{issueTitle}\" style=\"width:100%;height:250px;\"",
            html);

        for (var pageIndex = 0; pageIndex < issuePageNames.Length; pageIndex++)
        {
            var pageName = issuePageNames[pageIndex];
            var pageNumber = pageIndex + 1;
            var pagePath = $"/asset/images/event/m{issueNumber}/{pageName}";

            Assert.Contains(
                $"src=\"{pagePath}\" alt=\"{issueTitle} หน้า {pageNumber}\"",
                html);

            var imageResponse = await _client.GetAsync(pagePath);
            Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
            Assert.Equal("image/jpeg", imageResponse.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await imageResponse.Content.ReadAsByteArrayAsync());
        }
    }

    [Fact]
    public async Task InformationPageGroupsNewestIssueCardsWithoutVisualGaps()
    {
        const string rowMarker = "class=\"slider-items slider-width-col4 products-grid\"";
        var pageResponse = await _client.GetAsync("/Home/information");
        var html = WebUtility.HtmlDecode(await pageResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);

        int CardRowStart(int issueNumber)
        {
            var cardPosition = html.IndexOf($"href=\"#m{issueNumber}-modal\"", StringComparison.Ordinal);
            Assert.True(cardPosition >= 0, $"Issue {issueNumber} card was not rendered.");

            var rowPosition = html.LastIndexOf(rowMarker, cardPosition, StringComparison.Ordinal);
            Assert.True(rowPosition >= 0, $"Issue {issueNumber} card was not inside a product row.");
            return rowPosition;
        }

        var row19 = CardRowStart(19);
        var row20 = CardRowStart(20);
        var row21 = CardRowStart(21);
        var row22 = CardRowStart(22);
        var row23 = CardRowStart(23);

        Assert.Equal(row19, row20);
        Assert.Equal(row19, row21);
        Assert.NotEqual(row21, row22);
        Assert.Equal(row22, row23);
    }
}
