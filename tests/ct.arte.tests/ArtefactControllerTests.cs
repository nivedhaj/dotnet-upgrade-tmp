using Ct.Arte.Application.Dtos;
using Ct.Arte.Application.Services;
using Ct.Arte.Controllers;
using Ct.Arte.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ct.Arte.Tests;

[TestClass]
public class ArtefactControllerTests : TestBase
{
    private Mock<IArtefactService> _svc = null!;
    private ArtefactController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _svc = new Mock<IArtefactService>(MockBehavior.Strict);
        _ctrl = new ArtefactController(_svc.Object);
    }

    private static ArtefactDto SampleDto(string id = "a1", string title = "Doc")
        => new(
            Id: id,
            OwnerId: DefaultOwnerId,
            Title: title,
            Description: null,
            Url: "https://cdn.example/doc.pdf",
            MimeType: "application/pdf",
            FileName: "doc.pdf",
            FileSize: 1024,
            Sha1: null,
            Status: ArtefactStatus.Active,
            Model: null,
            CreatedOn: DateTimeOffset.UtcNow,
            CreatedBy: DefaultUserId,
            ModifiedOn: null,
            Tags: ["report"],
            MetaData: new Dictionary<string, string>(),
            PublishedOn: null);

    [TestMethod]
    public async Task Get_returns_Ok_with_search_results()
    {
        var list = new List<ArtefactDto> { SampleDto() };
        _svc.Setup(s => s.SearchAsync(It.IsAny<ArtefactSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _ctrl.Get(search: "doc", skip: 0, take: 10);
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [TestMethod]
    public async Task GetById_returns_NotFound_when_missing()
    {
        _svc.Setup(s => s.GetAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtefactDto?)null);

        var result = await _ctrl.GetById("missing", default);
        result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task GetById_returns_Ok_when_found()
    {
        var dto = SampleDto("found");
        _svc.Setup(s => s.GetAsync("found", It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _ctrl.GetById("found", default);
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(dto);
    }

    [TestMethod]
    public async Task Upsert_returns_created_dto()
    {
        var dto = SampleDto();
        _svc.Setup(s => s.UpsertAsync(It.IsAny<CreateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _ctrl.Upsert(new CreateArtefactRequest { Title = "Doc" }, default);
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(dto);
    }

    [TestMethod]
    public async Task Enable_returns_Ok_when_service_succeeds()
    {
        _svc.Setup(s => s.EnableAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _ctrl.Enable(new IdBody { Id = "a1" }, default);
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task Enable_returns_NotFound_when_service_fails()
    {
        _svc.Setup(s => s.EnableAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var result = await _ctrl.Enable(new IdBody { Id = "a1" }, default);
        result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task Publish_calls_PublishAsync_with_toPublic_false()
    {
        _svc.Setup(s => s.PublishAsync("a1", false, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _ctrl.Publish(new IdBody { Id = "a1" }, default);
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task UpdateTags_returns_Ok()
    {
        _svc.Setup(s => s.UpdateTagsAsync("a1", It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _ctrl.UpdateTags(new TagsBody { Id = "a1", Tags = ["x"] }, default);
        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task Widget_endpoints_return_Ok()
    {
        _svc.Setup(s => s.WidgetCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new { total = 1 });
        _svc.Setup(s => s.WidgetBandwidthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new { totalBytes = 0L, count = 0 });
        _svc.Setup(s => s.WidgetSizeBucketsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new { small = 0, medium = 0, large = 0 });
        _svc.Setup(s => s.WidgetAuthorCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<object>());
        _svc.Setup(s => s.WidgetFileCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new { files = 0, total = 0 });

        (await _ctrl.GetArtefactCountWidgetData(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetArtefactBandwidthWidgetData(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetWidgetSizeBucketsData(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetWidgetArtefactAuthorCountData(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetWidgetFileCount(default)).Should().BeOfType<OkObjectResult>();
    }
}
