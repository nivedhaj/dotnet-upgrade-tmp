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
public class GithubControllerExtendedTests : TestBase
{
    private Mock<IArtefactService> _svc = null!;
    private GithubController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _svc = new Mock<IArtefactService>(MockBehavior.Strict);
        _ctrl = new GithubController(_svc.Object);
    }

    [TestMethod]
    public async Task Enable_and_Disable_return_Ok()
    {
        _svc.Setup(s => s.EnableAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _svc.Setup(s => s.DisableAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        (await _ctrl.Enable(new IdBody { Id = "a1" }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.Disable(new IdBody { Id = "a1" }, default)).Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task GetFiles_and_FilesWithContent_search_github()
    {
        _svc.Setup(s => s.SearchAsync(
                It.Is<ArtefactSearchRequest>(r => r.Search == "github"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtefactDto>());

        (await _ctrl.GetFiles(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetFilesWithContent(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Download_returns_Ok_when_found()
    {
        var dto = new ArtefactDto(
            "a1", DefaultOwnerId, "spec", "content", "https://gh", null, "spec.json", 10, null,
            ArtefactStatus.Active, null, DateTimeOffset.UtcNow, DefaultUserId, null,
            new List<string> { "github" }, new Dictionary<string, string>(), null);
        _svc.Setup(s => s.GetAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _ctrl.Download("a1", default);
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public void GetSpecsFile_returns_Ok()
    {
        _ctrl.GetSpecsFile().Should().BeOfType<OkObjectResult>();
    }
}

[TestClass]
public class EtlControllerExtendedTests : TestBase
{
    private Mock<IArtefactService> _svc = null!;
    private EtlController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _svc = new Mock<IArtefactService>(MockBehavior.Strict);
        _ctrl = new EtlController(_svc.Object);
    }

    [TestMethod]
    public async Task GenerateCourseEtl_returns_scaffold()
    {
        _svc.Setup(s => s.GenerateEtlStubAsync("course", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { kind = "course", status = "generated" });

        (await _ctrl.CreateCourseEtl(default)).Should().BeOfType<OkObjectResult>();
    }
}

[TestClass]
public class AnonControllerExtendedTests : TestBase
{
    private Mock<IArtefactService> _svc = null!;
    private AnonController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _svc = new Mock<IArtefactService>(MockBehavior.Strict);
        _ctrl = new AnonController(_svc.Object);
    }

    [TestMethod]
    public async Task GetFilesFromUrl_searches()
    {
        _svc.Setup(s => s.SearchAsync(
                It.Is<ArtefactSearchRequest>(r => r.Take == 100),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtefactDto>());

        (await _ctrl.GetFilesFromUrl(search: "docs", default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetContentTags_returns_tags()
    {
        _svc.Setup(s => s.GetTagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "public", "lms" });

        (await _ctrl.GetContentTags(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Download_returns_Ok_when_found()
    {
        var dto = new ArtefactDto(
            "a1", DefaultOwnerId, "f", null, "https://cdn/f", null, "f.pdf", 1, null,
            ArtefactStatus.Active, null, DateTimeOffset.UtcNow, DefaultUserId, null,
            Array.Empty<string>(), new Dictionary<string, string>(), null);
        _svc.Setup(s => s.GetAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        (await _ctrl.Download("a1", default)).Should().BeOfType<OkObjectResult>();
    }
}
