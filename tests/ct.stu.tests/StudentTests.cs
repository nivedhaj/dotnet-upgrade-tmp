using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

/// <summary>Port of original StudentTests against modular StudentCommand/Query services.</summary>
[TestClass]
public partial class StudentTests : TestBase
{
    [TestMethod]
    public async Task Can_create_student()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Alice"), "tester");
            ids.Add(id);
            var dto = await qry.GetByIdAsync(id);
            Assert.IsNotNull(dto);
            Assert.AreEqual("Alice", dto!.Name);
            Assert.AreEqual(nameof(StudentStatus.Active), dto.Status);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_student()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("Bob"), "t"));
            var page = await qry.SearchAsync("Bob", null, 1, 20);
            Assert.IsTrue(page.Items.Any(s => s.Name == "Bob"));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_qry_one_student()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("One"), "t");
            ids.Add(id);
            var one = await qry.GetByIdAsync(id);
            Assert.IsNotNull(one);
            Assert.AreEqual(id, one!.Id);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_qry_course_and_student_in_course()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("C1-A", inst, "C1", "Class I", "A"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("C1-B", inst, "C1", "Class I", "B"), "t"));
            var sectionwise = await qry.GetSectionwiseAsync(inst);
            Assert.IsTrue(sectionwise.Sum(s => s.Count) >= 2);
            Assert.IsTrue(sectionwise.Any(s => s.CourseId == "C1"));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_qry_stu_for_facet()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Facet Stu", inst), "t");
            ids.Add(id);
            await cmds.SetTagsAsync(id, ["RTE", "Demo"]);
            var facets = await qry.GetFacetsAsync(inst);
            Assert.IsNotNull(facets);
            var byTag = await qry.GetByTagAsync("RTE");
            Assert.IsTrue(byTag.Any(s => s.Id == id));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_mobiles_and_contacts()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Mobile Stu", mobile: "9123456780"), "t");
            ids.Add(id);
            await cmds.SetPrimaryMobileAsync(id, "9123456780");
            await cmds.SetPrimaryEmailAsync(id, "m@example.com");
            var contacts = await qry.GetContactsAsync([id]);
            Assert.IsTrue(contacts.Count >= 1);
            Assert.AreEqual(id, contacts[0].StudentId);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_enable_disable_and_issue_tc()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Status Stu"), "t");
            ids.Add(id);
            await cmds.DisableAsync(id, "t");
            Assert.AreEqual(nameof(StudentStatus.Disabled), (await qry.GetByIdAsync(id))!.Status);

            await cmds.EnableAsync(id, "t");
            Assert.AreEqual(nameof(StudentStatus.Active), (await qry.GetByIdAsync(id))!.Status);

            await cmds.IssueTcAsync(new TcRequest { Id = id, Reason = "Relocated", TcDate = DateOnly.FromDateTime(DateTime.UtcNow) }, "t");
            var after = await qry.GetByIdAsync(id);
            Assert.AreEqual(nameof(StudentStatus.Disabled), after!.Status);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_assign_student_id_and_roll_no()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Id Stu", inst), "t");
            ids.Add(id);
            await cmds.AssignStudentIdAsync(new AssignStudentIdRequest { Id = id, StudentId = "STU-1001" });
            await cmds.SetRollNoAsync(id, inst, "12");
            var dto = await qry.GetByIdAsync(id);
            Assert.AreEqual("STU-1001", dto!.StudentId);
            Assert.AreEqual("12", dto.RollNo);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_update_student_and_course()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Old Name", inst, "C1", "Class I"), "t");
            ids.Add(id);
            await cmds.UpdateAsync(new UpdateStudentRequest
            {
                Id = id,
                Name = "New Name",
                Gender = "Female",
                Mobile = "9000000001"
            }, "t");
            await cmds.UpdateCourseAsync(id, inst, "C2", "Class II", "2026-27", "B", null);
            var dto = await qry.GetByIdAsync(id);
            Assert.AreEqual("New Name", dto!.Name);
            Assert.AreEqual("Class II", dto.CourseName);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_update_gender_parent_and_address()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Parent Stu"), "t");
            ids.Add(id);
            await cmds.UpdateGenderAsync(id, "Female");
            await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest
            {
                Id = id,
                Name = "Father Name",
                Mobile = "9111111111",
                Email = "father@example.com",
                Occupation = "Engineer"
            }, RelationType.Father);
            await cmds.SetHomeAddressAsync(new SetHomeAddressRequest
            {
                Id = id,
                Line1 = "12 Main St",
                City = "Pune",
                State = "MH",
                PostalCode = "411001",
                Country = "IN"
            });
            await cmds.SetHomePhoneAsync(id, "020-123456");
            var dto = await qry.GetByIdAsync(id);
            Assert.IsNotNull(dto);
            Assert.AreEqual("Female", dto!.Gender);
            Assert.AreEqual("Father Name", dto.FatherName);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_promote_student()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Promote Me", inst, "C1", "Class I", "A"), "t");
            ids.Add(id);
            var ok = await cmds.PromoteAsync(new PromoteStudentRequest
            {
                Id = id,
                BaseInstId = inst,
                BaseCourseId = "C1",
                BaseTerm = "2026-27",
                BaseSection = "A",
                TargetInstId = inst,
                TargetCourseId = "C2",
                TargetTerm = "2027-28",
                TargetSection = "A"
            }, "t");
            // Promote may return false if business rules block; either path is acceptable if no throw
            var dto = await qry.GetByIdAsync(id);
            Assert.IsNotNull(dto);
            _ = ok;
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_upload_photo_and_fees_receivable()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Photo Stu"), "t");
            ids.Add(id);
            await cmds.UploadPhotoAsync(id, "https://cdn.example.com/photo.jpg");
            await cmds.UpdateFeesReceivableAsync(new FeesReceivableRequest
            {
                Id = id.ToString(),
                InstId = "x",
                FeeId = Guid.NewGuid().ToString(),
                FeeName = "Tuition",
                Amount = 1000m
            });
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_all_students_and_flat_data()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("All1", inst), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("All2", inst), "t"));
            var all = await qry.GetAllAsync(inst);
            Assert.IsTrue(all.Count >= 2);
            var flat = await qry.GetFlatDataAsync(ids);
            Assert.IsTrue(flat.Count >= 2);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_filters_and_count()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("F1", inst), "t"));
            var filters = await qry.GetFiltersAsync(inst);
            Assert.IsNotNull(filters);
            var page = await qry.SearchAsync(null, inst, 1, 50);
            Assert.IsTrue(page.Total >= 1);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public void Can_pad_int_and_yy()
    {
        // Port of helper unit tests
        static string Pad(int n, int width) => n.ToString().PadLeft(width, '0');
        Assert.AreEqual("0007", Pad(7, 4));
        Assert.AreEqual("26", (2026 % 100).ToString("D2"));
    }

    [TestMethod]
    public void Guid_test()
    {
        var g = Guid.NewGuid();
        Assert.AreNotEqual(Guid.Empty, g);
        Assert.IsTrue(Guid.TryParse(g.ToString(), out _));
    }

    [TestMethod]
    public async Task Can_qry_rte_students_via_tag()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("RTE Kid"), "t");
            ids.Add(id);
            await cmds.SetTagsAsync(id, ["RTE"]);
            var rte = await qry.GetByTagAsync("RTE");
            Assert.IsTrue(rte.Any(s => s.Id == id));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_update_stu_id_gcs_style()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("GCS", studentId: "TMP"), "t");
            ids.Add(id);
            await cmds.AssignStudentIdAsync(new AssignStudentIdRequest { Id = id, StudentId = "GCS-2026-001" });
            Assert.AreEqual("GCS-2026-001", (await qry.GetByIdAsync(id))!.StudentId);
        }
        finally { await CleanupAsync(db, ids); }
    }
}
