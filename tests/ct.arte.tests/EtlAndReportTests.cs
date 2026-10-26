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
public class EtlControllerTests : TestBase
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
    public async Task GenerateStudentEtl_returns_scaffold()
    {
        _svc.Setup(s => s.GenerateEtlStubAsync("student", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { kind = "student", status = "generated" });

        var result = await _ctrl.CreateStudentEtl(default);
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GenerateInstituteEtl_returns_scaffold()
    {
        _svc.Setup(s => s.GenerateEtlStubAsync("institute", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { kind = "institute", status = "generated" });

        var result = await _ctrl.CreateInstituteEtl(default);
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GenerateFeesEtl_returns_scaffold()
    {
        _svc.Setup(s => s.GenerateEtlStubAsync("fees", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { kind = "fees", status = "generated" });

        var result = await _ctrl.CreateFeesEtl(default);
        result.Should().BeOfType<OkObjectResult>();
    }
}

[TestClass]
public class ReportControllerTests : TestBase
{
    private Mock<IArtefactService> _svc = null!;
    private ReportController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _svc = new Mock<IArtefactService>(MockBehavior.Strict);
        _ctrl = new ReportController(_svc.Object);
    }

    [TestMethod]
    public async Task GetArtefactsReport_returns_all()
    {
        var list = new List<ArtefactDto>
        {
            new("1", DefaultOwnerId, "R1", null, null, null, null, 0, null,
                ArtefactStatus.Active, null, DateTimeOffset.UtcNow, DefaultUserId, null,
                Array.Empty<string>(), new Dictionary<string, string>(), null)
        };
        _svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await _ctrl.GetArtefactsReport(default);
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }
}

[TestClass]
public class AnonControllerTests : TestBase
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
    public async Task GetFiles_searches_with_take_100()
    {
        _svc.Setup(s => s.SearchAsync(
                It.Is<ArtefactSearchRequest>(r => r.Take == 100),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtefactDto>());

        var result = await _ctrl.GetFiles(search: null, default);
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Download_returns_NotFound_when_missing()
    {
        _svc.Setup(s => s.GetAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((ArtefactDto?)null);
        var result = await _ctrl.Download("x", default);
        result.Should().BeOfType<NotFoundResult>();
    }
}
