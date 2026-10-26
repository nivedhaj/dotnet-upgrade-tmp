using Ct.Acad.Application;
using Ct.Acad.Controllers;
using Ct.Acad.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

[TestClass]
public class LmsExtendedTests : TestBase
{
    private Infrastructure.AcadDbContext _db = null!;
    private IAcadUserContext _user = null!;
    private LmsService _lms = null!;
    private AssessmentService _assessments = null!;
    private LMSController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _user = CreateUser();
        _lms = new LmsService(_db, _user);
        _assessments = new AssessmentService(_db, _user);
        _ctrl = new LMSController(_lms, _assessments);
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    private async Task<Guid> SeedQuestion(string text = "Q?", string answer = "A")
    {
        dynamic created = await _lms.CreateQuestionAsync(new CreateQuestionRequest
        {
            Text = text,
            Options = ["A", "B", "C"],
            Answer = answer,
            Tags = ["math", "easy"],
            Subject = "Maths",
            Marks = 1
        });
        return (Guid)created.Id;
    }

    [TestMethod]
    [TestCategory("LMS")]
    public async Task Create_Update_Get_Submit_Random()
    {
        var id = await SeedQuestion("2+2?", "4");

        (await _ctrl.CreateQuestion(new CreateQuestionRequest
        {
            Text = "3+3?",
            Answer = "6",
            Options = ["5", "6"]
        }, default)).Should().BeOfType<OkObjectResult>();

        (await _ctrl.UpdateQuestion(id, new CreateQuestionRequest
        {
            Text = "2+2=?",
            Answer = "4",
            Options = ["3", "4", "5"],
            Tags = ["math"]
        }, default)).Should().BeOfType<OkResult>();

        (await _ctrl.GetQuestions("math", "Maths", false, default)).Should().BeOfType<OkObjectResult>();

        var submit = await _ctrl.SubmitAnswer(new AnswerRequest { QuestionId = id, Answer = "4" }, default);
        submit.Should().BeOfType<OkObjectResult>();

        (await _ctrl.GetRandomQuestions(5, "math", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.RemodelQuestion(id, new CreateQuestionRequest { Text = "Remodeled?", Answer = "X" }, default))
            .Should().BeOfType<OkResult>();
    }

    [TestMethod]
    [TestCategory("LMS")]
    public async Task Publish_Unpublish_Enable_Disable_Archive()
    {
        var id = await SeedQuestion();

        (await _ctrl.PublishQuestions([id], default)).Should().BeOfType<OkResult>();
        (await _ctrl.UnpublishQuestions([id], default)).Should().BeOfType<OkResult>();
        (await _ctrl.EnableQuestion(new IdRequest { Id = id }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.DisableQuestion(new IdRequest { Id = id }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.ArchiveQuestion(new IdRequest { Id = id }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.UnArchiveQuestion(new IdRequest { Id = id }, default)).Should().BeOfType<OkResult>();
    }

    [TestMethod]
    [TestCategory("LMS")]
    public async Task Tags_and_image_tags()
    {
        var id = await SeedQuestion();
        (await _ctrl.UpdateQuestionsTags(new UpdateTagsRequest { Ids = [id], Tags = ["algebra", "grade10"] }, default))
            .Should().BeOfType<OkResult>();

        (await _ctrl.GetTags(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.CreateTag(new TagRequest { Name = "geometry" }, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.CreateImageTag(new ImageTagRequest { Name = "diagram", ImageUrl = "https://cdn/x.png" }, default))
            .Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetImageTags(default)).Should().BeOfType<OkObjectResult>();

        // Enable/Disable tag need a real tag id from CreateTag
        dynamic tag = await _assessments.CreateTagAsync("temp-tag");
        Guid tagId = tag.Id;
        (await _ctrl.EnableTag(new IdRequest { Id = tagId }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.DisableTag(new IdRequest { Id = tagId }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.UpdateTag(new NameRequest { Id = tagId, Name = "temp-tag-2" }, default)).Should().BeOfType<OkResult>();
    }

    [TestMethod]
    [TestCategory("LMS")]
    public async Task Service_SetStatus_and_SubmitAnswer_wrong()
    {
        var id = await SeedQuestion("Capital?", "Paris");
        await _lms.SetStatusAsync(id, QuestionStatus.Published);

        var wrong = await _lms.SubmitAnswerAsync(id, "Lyon");
        ((bool)((dynamic)wrong!).correct).Should().BeFalse();
    }
}
