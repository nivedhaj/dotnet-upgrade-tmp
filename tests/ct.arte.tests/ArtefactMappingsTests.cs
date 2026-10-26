using Ct.Arte.Application.Dtos;
using Ct.Arte.Domain.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Arte.Tests;

[TestClass]
public class ArtefactMappingsTests : TestBase
{
    [TestMethod]
    public void ToEntity_maps_create_request()
    {
        var req = new CreateArtefactRequest
        {
            Title = "T",
            Description = "D",
            Url = "https://x",
            MimeType = "text/csv",
            FileName = "f.csv",
            FileSize = 99,
            Sha1 = "abc",
            Tags = ["t1"],
            MetaData = new Dictionary<string, string> { ["k"] = "v" }
        };

        var entity = req.ToEntity(DefaultOwnerId);

        entity.Title.Should().Be("T");
        entity.OwnerId.Should().Be(DefaultOwnerId);
        entity.Status.Should().Be(ArtefactStatus.Active);
        entity.Id.Should().NotBeNullOrEmpty();
        entity.Tags.Should().Contain("t1");
        entity.MetaData["k"].Should().Be("v");
    }

    [TestMethod]
    public void ToDto_maps_entity()
    {
        var entity = MakeArtefact(title: "Mapped", tags: "a", "b");
        var dto = entity.ToDto();

        dto.Id.Should().Be(entity.Id);
        dto.Title.Should().Be("Mapped");
        dto.Tags.Should().BeEquivalentTo(["a", "b"]);
        dto.Status.Should().Be(ArtefactStatus.Active);
    }

    [TestMethod]
    public void EnsureId_generates_guid_when_empty()
    {
        var e = new Artefact { Title = "x" };
        e.Id.Should().BeEmpty();
        var id = e.EnsureId();
        id.Should().NotBeNullOrEmpty();
        Guid.TryParse(id, out _).Should().BeTrue();
    }

    [TestMethod]
    public void AddTag_is_case_insensitive_unique()
    {
        var e = MakeArtefact();
        e.AddTag("GitHub");
        e.AddTag("github");
        e.Tags.Should().ContainSingle(t => t.Equals("GitHub", StringComparison.OrdinalIgnoreCase));
    }
}
