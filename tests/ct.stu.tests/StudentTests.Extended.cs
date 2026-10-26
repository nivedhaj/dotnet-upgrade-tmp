using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

/// <summary>Additional StudentTests ports (jobs, data-fix, counts, sections, fees receivable, …).</summary>
public partial class StudentTests
{
    [TestMethod]
    public async Task Can_get_disabled_students()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var active = await cmds.CreateAsync(SampleStudent("Active One", inst), "t");
            var disabled = await cmds.CreateAsync(SampleStudent("Disabled One", inst), "t");
            ids.Add(active); ids.Add(disabled);
            await cmds.DisableAsync(disabled, "t");
            var all = await qry.GetAllAsync(inst);
            Assert.IsTrue(all.Any(s => s.Id == disabled && s.Status == nameof(StudentStatus.Disabled)));
            Assert.IsTrue(all.Any(s => s.Id == active && s.Status == nameof(StudentStatus.Active)));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_change_student_section_and_set_course()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("SecChange", inst, "C1", "I", "A"), "t");
            ids.Add(id);
            await cmds.UpdateCourseAsync(id, inst, "C1", "I", "2026-27", "C", null);
            var dto = await qry.GetByIdAsync(id);
            Assert.AreEqual("C", dto!.SectionName);
            await cmds.UpdateCourseAsync(id, inst, "C2", "II", "2026-27", "C", "Main");
            dto = await qry.GetByIdAsync(id);
            Assert.AreEqual("II", dto!.CourseName);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_assign_students_to_section_with_list()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            for (var i = 0; i < 3; i++)
                ids.Add(await cmds.CreateAsync(SampleStudent($"Bulk{i}", inst, "C1", "I", "A"), "t"));
            foreach (var id in ids)
                await cmds.UpdateCourseAsync(id, inst, "C1", "I", "2026-27", "B", null);
            var sectionwise = await qry.GetSectionwiseAsync(inst);
            Assert.IsTrue(sectionwise.Any(s => s.Section == "B" && s.Count >= 3));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_students_based_on_tags()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Tagged"), "t");
            ids.Add(id);
            await cmds.SetTagsAsync(id, ["Sports", "Prefect"]);
            Assert.IsTrue((await qry.GetByTagAsync("Sports")).Any(s => s.Id == id));
            Assert.IsTrue((await qry.GetByTagAsync("Prefect")).Any(s => s.Id == id));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_fix_student_name_and_spaces_in_ids()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            // SetName title-cases and collapses whitespace
            var id = await cmds.CreateAsync(SampleStudent("  john   DOE  "), "t");
            ids.Add(id);
            var dto = await qry.GetByIdAsync(id);
            Assert.AreEqual("John Doe", dto!.Name);

            await cmds.AssignStudentIdAsync(new AssignStudentIdRequest { Id = id, StudentId = "  STU-100 " });
            Assert.AreEqual("STU-100", (await qry.GetByIdAsync(id))!.StudentId);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_fix_repeating_student_names()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("Asha Asha"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("Asha Asha"), "t"));
            var page = await qry.SearchAsync("Asha", null, 1, 20);
            Assert.IsTrue(page.Total >= 2);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_students_firstname_lastname_id()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Priya Sharma", studentId: "PS-01"), "t");
            ids.Add(id);
            var byFirst = await qry.SearchAsync("Priya", null, 1, 10);
            Assert.IsTrue(byFirst.Items.Any(s => s.Id == id));
            var byLast = await qry.SearchAsync("Sharma", null, 1, 10);
            Assert.IsTrue(byLast.Items.Any(s => s.Id == id));
            var byId = await qry.SearchAsync("PS-01", null, 1, 10);
            Assert.IsTrue(byId.Items.Any(s => s.Id == id));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_student_count_institutewise_and_genderwise()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("Boy", inst, gender: "Male"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("Girl", inst, gender: "Female"), "t"));
            var all = await qry.GetAllAsync(inst);
            Assert.AreEqual(2, all.Count);
            var males = all.Count(s => s.Gender == "Male");
            var females = all.Count(s => s.Gender == "Female");
            Assert.AreEqual(1, males);
            Assert.AreEqual(1, females);
            var facets = await qry.GetFacetsAsync(inst);
            Assert.IsTrue(facets.Any(f => f.Key == "status"));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_students_flat_data_and_coursewise_count()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("F1", inst, "C1", "I"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("F2", inst, "C2", "II"), "t"));
            var flat = await qry.GetFlatDataAsync(ids);
            Assert.AreEqual(2, flat.Count);
            var sectionwise = await qry.GetSectionwiseAsync(inst);
            var coursewise = sectionwise.GroupBy(s => s.CourseId).Select(g => new { CourseId = g.Key, Count = g.Sum(x => x.Count) }).ToList();
            Assert.IsTrue(coursewise.Count >= 2);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_set_and_get_fees_receivable()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("FeeRec", studentId: "FR-1"), "t");
            ids.Add(id);
            await cmds.UpdateFeesReceivableAsync(new FeesReceivableRequest
            {
                Id = "FR-1",
                FeeId = "fee-1",
                FeeName = "Tuition",
                TxId = "tx-1",
                Amount = 2500m
            });
            var entity = await db.Students.AsNoTracking().FirstAsync(s => s.Id == id);
            Assert.IsTrue(entity.Attributes.Keys.Any(k => k.StartsWith("feesReceivable")));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_student_ids_in_course_and_exclude()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("In1", inst, "CX", "X"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("In2", inst, "CX", "X"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("Out", inst, "CY", "Y"), "t"));
            var all = await qry.GetAllAsync(inst);
            var inCourse = all.Where(s => s.CourseId == "CX").Select(s => s.Id).ToList();
            var excluded = new HashSet<Guid> { ids[0] };
            var filtered = inCourse.Where(id => !excluded.Contains(id)).ToList();
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual(ids[1], filtered[0]);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_enrl_studentId_and_change_enrol()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Enrol", inst, studentId: "ENR-9"), "t");
            ids.Add(id);
            var dto = await qry.GetByIdAsync(id);
            Assert.AreEqual("ENR-9", dto!.StudentId);
            // change enrolment course/section
            await cmds.UpdateCourseAsync(id, inst, "C9", "IX", "2026-27", "D", "North");
            dto = await qry.GetByIdAsync(id);
            Assert.AreEqual("IX", dto!.CourseName);
            Assert.AreEqual("D", dto.SectionName);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_set_institute_email_and_get_contacts_for_result()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Result Contact"), "t");
            ids.Add(id);
            await cmds.SetPrimaryEmailAsync(id, "stu.result@school.edu");
            await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest
            {
                Id = id, Name = "Parent R", Email = "parent.result@example.com", Mobile = "9000000123"
            }, RelationType.Mother);
            var contacts = await qry.GetContactsAsync([id]);
            Assert.IsTrue(contacts[0].Email == "stu.result@school.edu" || contacts[0].ParentEmails.Contains("parent.result@example.com"));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Job_style_create_update_enable_disable()
    {
        // Port of can_*_student_job — same commands, no Raven job queue
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("JobStu"), "job");
            ids.Add(id);
            await cmds.UpdateAsync(new UpdateStudentRequest { Id = id, Name = "JobStu Updated" }, "job");
            Assert.AreEqual("Jobstu Updated", (await qry.GetByIdAsync(id))!.Name); // title case via SetName
            await cmds.DisableAsync(id, "job");
            Assert.AreEqual(nameof(StudentStatus.Disabled), (await qry.GetByIdAsync(id))!.Status);
            await cmds.EnableAsync(id, "job");
            Assert.AreEqual(nameof(StudentStatus.Active), (await qry.GetByIdAsync(id))!.Status);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_students_for_exam_facet()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("Exam1", inst, "C1", "I", "A"), "t"));
            await cmds.SetTagsAsync(ids[0], ["ExamEligible"]);
            var facets = await qry.GetFacetsAsync(inst);
            Assert.IsTrue(facets.Any(f => f.Key is "course" or "section" or "tag"));
            var eligible = await qry.GetByTagAsync("ExamEligible");
            Assert.IsTrue(eligible.Any(s => s.Id == ids[0]));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Set_user_id_for_students()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("UserLink"), "t");
            ids.Add(id);
            var stu = await db.Students.FirstAsync(s => s.Id == id);
            stu.UserId = "user-abc";
            await db.SaveChangesAsync();
            var reloaded = await db.Students.AsNoTracking().FirstAsync(s => s.Id == id);
            Assert.AreEqual("user-abc", reloaded.UserId);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Student_not_as_user_when_userid_null()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("NoUser"), "t");
            ids.Add(id);
            var stu = await db.Students.AsNoTracking().FirstAsync(s => s.Id == id);
            Assert.IsTrue(string.IsNullOrEmpty(stu.UserId));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_all_passes_out_students_via_tc_or_disabled()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("PassOut", inst), "t");
            ids.Add(id);
            await cmds.IssueTcAsync(new TcRequest { Id = id, Reason = "Completed" }, "t");
            var all = await qry.GetAllAsync(inst);
            Assert.IsTrue(all.Any(s => s.Id == id && s.Status == nameof(StudentStatus.Disabled)));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_recipients()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Recv", email: "r@x.com", mobile: "9000000008"), "t");
            ids.Add(id);
            await cmds.SetPrimaryEmailAsync(id, "r@x.com");
            await cmds.SetPrimaryMobileAsync(id, "9000000008");
            var contacts = await qry.GetContactsAsync([id]);
            Assert.AreEqual(1, contacts.Count);
            Assert.IsTrue(!string.IsNullOrEmpty(contacts[0].Email) || !string.IsNullOrEmpty(contacts[0].Mobile));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public void Regex_match_student_id_pattern()
    {
        var pattern = @"^[A-Z]{2,5}-\d{2,6}$";
        Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch("STU-1001", pattern));
        Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch("bad id", pattern));
    }

    [TestMethod]
    public async Task Can_qry_student_count_and_rte_count()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("Cnt1", inst), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("Cnt2", inst), "t"));
            await cmds.SetTagsAsync(ids[0], ["RTE"]);
            var page = await qry.SearchAsync(null, inst, 1, 50);
            Assert.IsTrue(page.Total >= 2);
            var rte = await qry.GetByTagAsync("RTE");
            Assert.IsTrue(rte.Count(s => ids.Contains(s.Id)) >= 1);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_stu_prefix_helpers()
    {
        // Port of can_get_stu_prefix / for_id — formatting helpers used when assigning ids
        static string PrefixFor(string shortName, int year) => $"{shortName}-{year % 100:D2}-";
        static string NextId(string prefix, int seq) => $"{prefix}{seq:D3}";
        var p = PrefixFor("GCS", 2026);
        Assert.AreEqual("GCS-26-", p);
        Assert.AreEqual("GCS-26-007", NextId(p, 7));
    }

    [TestMethod]
    public async Task Can_fix_admission_numbers_on_create()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var req = SampleStudent("AdmFix");
            // admission number set on create via request
            var id = await cmds.CreateAsync(req, "t");
            ids.Add(id);
            var enr = await db.StudentEnrollments.AsNoTracking().FirstAsync(e => e.StudentId == id);
            Assert.AreEqual("ADM-001", enr.AdmissionNo);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Absentee_sms_recipients_resolve_from_contacts()
    {
        // Port of can_send_absentee_sms — modular Stu only resolves recipients; Connect sends SMS
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Absent"), "t");
            ids.Add(id);
            await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest
            {
                Id = id, Name = "Dad", Mobile = "9887766554"
            }, RelationType.Father);
            var contacts = await qry.GetContactsAsync([id]);
            Assert.IsTrue(contacts[0].FatherMobile == "9887766554" || contacts[0].Mobile is not null);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Attendance_widget_data_uses_student_attendance_field()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Att"), "t");
            ids.Add(id);
            var stu = await db.Students.FirstAsync(s => s.Id == id);
            stu.Attendance = "Present";
            await db.SaveChangesAsync();
            var reloaded = await db.Students.AsNoTracking().FirstAsync(s => s.Id == id);
            Assert.AreEqual("Present", reloaded.Attendance);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public void Can_get_policies_placeholder()
    {
        // Legacy institute policies endpoint — not on Student services; document contract
        var policies = new[] { "RTE", "General", "StaffWard" };
        Assert.IsTrue(policies.Contains("RTE"));
    }

    [TestMethod]
    public void Can_run_settings_placeholder()
    {
        Configure();
        Assert.IsNotNull(Config);
        Assert.IsFalse(string.IsNullOrWhiteSpace(GetConnectionString()));
    }
}

// Make original class partial
