using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Repo.Db.Index;
using Repo.Resource;
using Repo.Dto;
using ct._Core.Repo.Resource;

namespace ct._Core.Persistence;

public sealed class PostgresQuery<T> : IEnumerable<T>
{
    private readonly PostgresDocumentSession _session;
    private readonly List<PostgresRow<T>> _rows;
    private readonly Type? _indexType;
    private readonly IOrderedEnumerable<PostgresRow<T>>? _ordered;

    internal PostgresQuery(PostgresDocumentSession session, IEnumerable<PostgresRow<T>> rows, Type? indexType, IOrderedEnumerable<PostgresRow<T>>? ordered = null)
    {
        _session = session;
        _rows = rows.ToList();
        _indexType = indexType;
        _ordered = ordered;
    }

    private PostgresQuery<T> New(IEnumerable<PostgresRow<T>> rows) => new(_session, rows, _indexType);

    public PostgresQuery<T> Where(Func<T, bool> predicate) => New(_rows.Where(x => predicate(x.Value)));
    public PostgresQuery<T> Where(Expression<Func<T, bool>> predicate) => New(_rows.Where(x => predicate.Compile()(x.Value)));
    public PostgresQuery<T> OrderBy<TKey>(Func<T, TKey> keySelector)
    {
        var ordered = _rows.OrderBy(x => keySelector(x.Value));
        return new PostgresQuery<T>(_session, ordered, _indexType, ordered);
    }
    public PostgresQuery<T> OrderByDescending<TKey>(Func<T, TKey> keySelector)
    {
        var ordered = _rows.OrderByDescending(x => keySelector(x.Value));
        return new PostgresQuery<T>(_session, ordered, _indexType, ordered);
    }
    public PostgresQuery<T> ThenBy<TKey>(Func<T, TKey> keySelector)
    {
        var ordered = (_ordered ?? _rows.OrderBy(_ => 0)).ThenBy(x => keySelector(x.Value));
        return new PostgresQuery<T>(_session, ordered, _indexType, ordered);
    }
    public PostgresQuery<T> ThenByDescending<TKey>(Func<T, TKey> keySelector)
    {
        var ordered = (_ordered ?? _rows.OrderBy(_ => 0)).ThenByDescending(x => keySelector(x.Value));
        return new PostgresQuery<T>(_session, ordered, _indexType, ordered);
    }

    public PostgresQuery<T> Search<TKey>(Func<T, TKey> selector, string term)
    {
        var text = term?.Trim().Trim('"') ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text)) return this;
        return New(_rows.Where(x => (selector(x.Value)?.ToString() ?? string.Empty)
            .Contains(text, StringComparison.OrdinalIgnoreCase)));
    }

    public IEnumerable<TResult> Select<TResult>(Func<T, TResult> selector) => _rows.Select(x => selector(x.Value));

    public IEnumerable<TResult> OfType<TResult>()
    {
        foreach (var row in _rows)
        {
            var mapped = PostgresProjection.Project<TResult>(row.Document, row.Value, _indexType);
            if (mapped is TResult result) yield return result;
        }
    }

    public T? FirstOrDefault() => _rows.Select(x => x.Value).FirstOrDefault();
    public T First() => _rows.Select(x => x.Value).First();
    public T SingleOrDefault() => _rows.Select(x => x.Value).SingleOrDefault()!;
    public bool Any() => _rows.Count != 0;
    public bool Any(Func<T, bool> predicate) => _rows.Any(x => predicate(x.Value));
    public int Count() => _rows.Count;
    public PostgresQuery<T> Skip(int count) => New(_rows.Skip(Math.Max(0, count)));
    public PostgresQuery<T> Take(int count) => New(_rows.Take(Math.Max(0, count)));
    public PostgresQuery<T> Statistics(out QueryStatistics statistics)
    {
        statistics = new QueryStatistics { TotalResults = _rows.Count };
        return this;
    }
    public List<T> ToList() => _rows.Select(x => x.Value).ToList();
    public T[] ToArray() => _rows.Select(x => x.Value).ToArray();
    public IEnumerator<T> GetEnumerator() => _rows.Select(x => x.Value).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class PostgresDocumentQuery<T, TIndex> : IEnumerable<T>
{
    private readonly PostgresQuery<T> _query;
    private readonly PostgresDocumentSession _session;

    internal PostgresDocumentQuery(IEnumerable<PostgresRow<T>> rows, PostgresDocumentSession session)
    {
        _query = new PostgresQuery<T>(session, rows, typeof(TIndex));
        _session = session;
    }

    public PostgresDocumentQuery<T, TIndex> WhereLucene(string field, string predicate)
    {
        var rows = _query.Where(x => PostgresLuceneMatcher.Matches(x, field, predicate));
        return new PostgresDocumentQuery<T, TIndex>(rows, _session);
    }

    public IEnumerable<TResult> OfType<TResult>() => _query.OfType<TResult>();
    public PostgresQuery<T> Where(Expression<Func<T, bool>> predicate) => _query.Where(predicate);
    public IEnumerator<T> GetEnumerator() => _query.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class PostgresLuceneMatcher
{
    public static bool Matches<T>(T item, string field, string predicate)
    {
        if (string.IsNullOrWhiteSpace(predicate)) return true;
        var groups = predicate.Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return groups.Any(group => MatchAndGroup(item, field, group));
    }

    private static bool MatchAndGroup<T>(T item, string field, string predicate)
    {
        var text = predicate.Replace("(", " ").Replace(")", " ");
        var fieldValue = GetValue(item, field);
        var haystack = fieldValue?.ToString() ?? string.Empty;
        var sawClause = false;
        foreach (var token in text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Equals("AND", StringComparison.OrdinalIgnoreCase)) continue;
            var split = token.Split(':', 2);
            if (split.Length != 2) continue;
            sawClause = true;
            var expected = split[1].Trim('"', '*');
            var actual = GetValue(item, split[0]);
            if (actual is IEnumerable<string> list)
            {
                if (!list.Any(x => x.Contains(expected, StringComparison.OrdinalIgnoreCase))) return false;
            }
            else if (!(actual?.ToString() ?? string.Empty).Contains(expected, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return sawClause || haystack.Contains(text.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static object? GetValue(object item, string name)
    {
        var p = item.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return p?.GetValue(item);
    }
}

internal static class PostgresIndexFactory
{
    public static IEnumerable<PostgresRow<T>> Build<T, TIndex>(PostgresDocumentSession session)
    {
        var index = typeof(TIndex);
        if (index == typeof(Student_Inst_Index))
            return BuildStudentIndex<T>(session);
        if (index == typeof(Student_Coursewise_Count))
            return BuildCoursewiseCount<T>(session);
        if (index == typeof(Inst_Student_Count))
            return BuildInstituteCount<T>(session);
        if (index == typeof(AttendanceEvent_IX))
            return BuildAttendance<T>(session);
        if (index == typeof(Student_Essentials_IX))
            return BuildEssentials<T>(session);
        if (index == typeof(Ping_Index))
            return BuildSimple<T>(session, typeof(Ping));
        if (index == typeof(Course_Structure_Index))
            return BuildSimple<T>(session, typeof(Course));
        if (index == typeof(Coursewise_Sectionwise_Student_Index))
            return BuildSimple<T>(session, typeof(Course));
        return Array.Empty<PostgresRow<T>>();
    }

    private static IEnumerable<PostgresRow<T>> BuildSimple<T>(PostgresDocumentSession session, Type documentType)
    {
        var raw = LoadTyped(session, documentType);
        return raw.Select(x => new PostgresRow<T>((T)x, x));
    }

    private static IEnumerable<PostgresRow<T>> BuildStudentIndex<T>(PostgresDocumentSession session)
    {
        var students = LoadTyped(session, typeof(Student)).Cast<Student>().ToList();
        var courses = LoadTyped(session, typeof(Course)).Cast<Course>().ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var result = new List<PostgresRow<T>>();
        foreach (var student in students)
        {
            foreach (var e in student.Enrollments ?? new List<Enrollment>())
            {
                courses.TryGetValue(e.CourseId ?? string.Empty, out var course);
                var row = new Student_Inst_Index.Result
                {
                    Id = student.Id,
                    Name = student.Name,
                    Gender = student.Gender,
                    Status = student.Status,
                    EnrollmentStatus = e.Status,
                    InstId = e.InstId,
                    StudentId = e.StudentId,
                    CourseId = e.CourseId,
                    CourseName = course?.Name ?? e.CourseName,
                    Branch = course?.Branch,
                    CourseSortIndex = course?.SortIndex ?? 0,
                    TermName = e.TermName,
                    SectionName = e.SectionName,
                    RollNo = e.RollNoAsInt,
                    Tags = student.Tags ?? new List<string>(),
                    Emails = student.EmailCSV,
                    EmailList = student.EmailCSV?.Split(',', StringSplitOptions.None).ToList(),
                    Mobiles = student.MobileCSV,
                    FatherName = student.Father?.Name,
                    MotherName = student.Mother?.Name,
                    FatherPAN = student.Father?.PAN,
                    MotherPAN = student.Mother?.PAN,
                    VirtualId = student.VirtualId,
                    OrgId = null,
                    DOA = e.DOA,
                    DOB = student.DOB,
                    UserId = student.UserId,
                    AdmissionNo = e.AdmissionNo,
                    FeesReceivable = student.FeesReceivable ?? new List<FeesReceivable>()
                };
                if (typeof(T) == typeof(Student_Inst_Index.Result)) result.Add(new PostgresRow<T>((T)(object)row, student));
                else result.Add(new PostgresRow<T>((T)(object)row, student));
            }
        }
        return result;
    }

    private static IEnumerable<PostgresRow<T>> BuildCoursewiseCount<T>(PostgresDocumentSession session)
    {
        var students = LoadTyped(session, typeof(Student)).Cast<Student>();
        var grouped = students.Where(s => s.Status == StudentStatusEnum.Active)
            .SelectMany(s => (s.Enrollments ?? new()).Where(e => e.Status == EnrollmentStatusEnum.Active)
                .Select(e => new { s, e }))
            .GroupBy(x => new { x.e.InstId, x.e.CourseId, x.e.CourseName, x.e.TermName, x.e.SectionName });
        return grouped.Select(g =>
        {
            var row = new Student_Coursewise_Count.Result
            {
                InstId = g.Key.InstId,
                CourseId = g.Key.CourseId,
                CourseName = g.Key.CourseName,
                TermName = g.Key.TermName,
                SectionName = g.Key.SectionName,
                Boys = g.Count(x => x.s.Gender == GenderEnum.Male),
                Girls = g.Count(x => x.s.Gender == GenderEnum.Female)
            };
            return new PostgresRow<T>((T)(object)row, row);
        });
    }

    private static IEnumerable<PostgresRow<T>> BuildInstituteCount<T>(PostgresDocumentSession session)
    {
        var students = LoadTyped(session, typeof(Student)).Cast<Student>();
        return students.Where(s => s.Status == StudentStatusEnum.Active)
            .SelectMany(s => (s.Enrollments ?? new()).Where(e => e.Status == EnrollmentStatusEnum.Active)
                .Select(e => new { s, e }))
            .GroupBy(x => x.e.InstId)
            .Select(g =>
            {
                var row = new Inst_Student_Count.Result
                {
                    InstId = g.Key,
                    StudentCountMale = g.Count(x => x.s.Gender == GenderEnum.Male),
                    StudentCountFemale = g.Count(x => x.s.Gender == GenderEnum.Female)
                };
                return new PostgresRow<T>((T)(object)row, row);
            });
    }

    private static IEnumerable<PostgresRow<T>> BuildAttendance<T>(PostgresDocumentSession session)
    {
        var events = LoadTyped(session, typeof(AttendanceEvent)).Cast<AttendanceEvent>();
        return events.Select(a => new PostgresRow<T>((T)(object)new AttendanceEvent_IX.Result
        {
            InstId = a.InstId,
            CourseId = a.CourseId,
            StudentId = a.StudentId,
            Date = a.Date
        }, a));
    }

    private static IEnumerable<PostgresRow<T>> BuildEssentials<T>(PostgresDocumentSession session)
    {
        var values = LoadTyped(session, typeof(StudentEssentials)).Cast<StudentEssentials>();
        return values.Select(x => new PostgresRow<T>((T)(object)new Student_Essentials_IX.Result { Id = x.Id, StudentId = x.StudentId }, x));
    }

    private static List<object> LoadTyped(PostgresDocumentSession session, Type type)
    {
        var method = typeof(PostgresDocumentSession).GetMethod("LoadAll", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(Type) }, null)!;
        var generic = method.MakeGenericMethod(type);
        var rows = (IEnumerable)generic.Invoke(session, new object[] { type, type })!;
        var list = new List<object>();
        foreach (var row in rows)
            list.Add(row.GetType().GetProperty("Document")!.GetValue(row)!);
        return list;
    }
}

internal static class PostgresProjection
{
    public static TResult? Project<TResult>(object document, object value, Type? indexType)
    {
        if (document is TResult exact) return exact;
        if (typeof(TResult) == typeof(Student)) return document as TResult;
        if (typeof(TResult) == typeof(AttendanceEvent)) return document as TResult;
        if (typeof(TResult) == typeof(StudentEssentials)) return document as TResult;
        if (typeof(TResult) == typeof(Ping)) return document as TResult;
        if (indexType == typeof(Student_Inst_Index))
        {
            if (document is Student student)
            {
                if (typeof(TResult) == typeof(Rpt_StudentDto))
                {
                    var row = value as Student_Inst_Index.Result;
                    var enrollment = student.Enrollments?.FirstOrDefault(e => e.InstId == row?.InstId);
                    return (TResult)(object)new Rpt_StudentDto
                    {
                        Id = student.Id,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        DOB = student.DOB,
                        Gender = student.Gender.ToString(),
                        Father = student.Father,
                        Mother = student.Mother,
                        InstId = row?.InstId,
                        StudentId = row?.StudentId,
                        Emails = row?.Emails,
                        Mobiles = row?.Mobiles,
                        Enrollments = student.Enrollments,
                        CourseId = row?.CourseId,
                        CourseName = row?.CourseName,
                        Branch = row?.Branch,
                        SectionName = row?.SectionName,
                        RollNo = enrollment?.RollNo
                    };
                }
                if (typeof(TResult) == typeof(Student)) return (TResult)(object)student;
            }
        }
        return default;
    }
}

public sealed class QueryStatistics
{
    public int TotalResults { get; set; }
}
