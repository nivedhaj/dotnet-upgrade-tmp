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
public class GithubControllerTests : TestBase
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
    public async Task Get_searches_github_tag()
    {
        _svc.Setup(s => s.SearchAsync(
                It.Is<ArtefactSearchRequest>(r => r.Search == "github" && r.Take == 100),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtefactDto>());

        var result = await _ctrl.Get(default);
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Upsert_adds_github_tag()
    {
        CreateArtefactRequest? captured = null;
        _svc.Setup(s => s.UpsertAsync(It.IsAny<CreateArtefactRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateArtefactRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ArtefactDto(
                "id", DefaultOwnerId, "repo", null, null, null, null, 0, null,
                ArtefactStatus.Active, null, DateTimeOffset.UtcNow, DefaultUserId, null,
                new List<string> { "github" }, new Dictionary<string, string>(), null));

        await _ctrl.Upsert(new CreateArtefactRequest { Title = "repo" }, default);

        captured.Should().NotBeNull();
        captured!.Tags.Should().Contain("github");
    }

    [TestMethod]
    public async Task Download_returns_NotFound_when_missing()
    {
        _svc.Setup(s => s.GetAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((ArtefactDto?)null);
        var result = await _ctrl.Download("x", default);
        result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task UpdateDescription_returns_Ok()
    {
        _svc.Setup(s => s.UpdateDescriptionAsync("a1", "new desc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _ctrl.UpdateDescription(new DescriptionBody { Id = "a1", Description = "new desc" }, default);
        result.Should().BeOfType<OkResult>();
    }
}
