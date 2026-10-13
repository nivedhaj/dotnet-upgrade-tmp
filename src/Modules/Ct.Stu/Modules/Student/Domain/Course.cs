namespace Ct.Modules.Admissions.Domain;

/// <summary>Legacy `Course` (spec section 18) normalized into Course/Term/Section/Subject.</summary>
public class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InstId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Branch { get; set; }
    public string? Affiliation { get; set; }
    public int? EduLevel { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Active;
    public int SortIndex { get; set; }
    public int Rank { get; set; }

    /// <summary>JSONB — genuinely dynamic per-course ordering data (spec section 113).</summary>
    public List<string> ExamSubjectOrder { get; set; } = new();
    public string? MarksCardTemplate { get; set; }
    public Dictionary<string, object?> Attributes { get; set; } = new();

    public List<CourseTerm> Terms { get; private set; } = new();

    /// <summary>Legacy "Add section" rule (spec section 19): reject duplicate names case-insensitively.</summary>
    public CourseSection AddSection(CourseTerm term, string name)
    {
        if (term.Sections.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Section '{name}' already exists on term '{term.Name}'.");
        }

        var section = new CourseSection { Id = Guid.NewGuid(), TermId = term.Id, Name = name, SortOrder = term.Sections.Count };
        term.Sections.Add(section);
        return section;
    }

    /// <summary>Legacy "Rename section" rule: reject missing section or empty new name.</summary>
    public void RenameSection(CourseTerm term, Guid sectionId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("New section name must not be empty.", nameof(newName));
        }

        var section = term.Sections.FirstOrDefault(s => s.Id == sectionId)
            ?? throw new InvalidOperationException("Section not found.");
        section.Name = newName;
    }

    /// <summary>Legacy "Remove section" rule: reject missing section.</summary>
    public void RemoveSection(CourseTerm term, Guid sectionId)
    {
        var section = term.Sections.FirstOrDefault(s => s.Id == sectionId)
            ?? throw new InvalidOperationException("Section not found.");
        term.Sections.Remove(section);
    }

    /// <summary>Legacy "Add subject" rule: reject empty or duplicate subject.</summary>
    public CourseSubject AddSubject(CourseSection section, string name, bool optional)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Subject name must not be empty.", nameof(name));
        }

        if (section.Subjects.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Subject '{name}' already exists on section '{section.Name}'.");
        }

        var subject = new CourseSubject { Id = Guid.NewGuid(), SectionId = section.Id, Name = name, Optional = optional, SortOrder = section.Subjects.Count };
        section.Subjects.Add(subject);
        return subject;
    }

    /// <summary>Legacy "Rename subject" rule: only rename when subject exists and new name is non-empty.</summary>
    public void RenameSubject(CourseSection section, Guid subjectId, string newName)
    {
        var subject = section.Subjects.FirstOrDefault(s => s.Id == subjectId);
        if (subject is null || string.IsNullOrWhiteSpace(newName))
        {
            return;
        }
        subject.Name = newName;
    }

    /// <summary>Legacy "Remove subject" rule: reject empty/missing subject.</summary>
    public void RemoveSubject(CourseSection section, Guid subjectId)
    {
        var subject = section.Subjects.FirstOrDefault(s => s.Id == subjectId)
            ?? throw new InvalidOperationException("Subject not found.");
        section.Subjects.Remove(subject);
    }
}

public class CourseTerm
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int SortOrder { get; set; }

    public List<CourseSection> Sections { get; private set; } = new();
}

public class CourseSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TermId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public List<CourseSubject> Subjects { get; private set; } = new();
}

public class CourseSubject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Optional { get; set; }
    public int SortOrder { get; set; }
}
