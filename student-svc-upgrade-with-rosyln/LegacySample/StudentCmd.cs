using Raven.Client.Documents.Session;

public sealed class StudentCmd
{
    private readonly IDocumentSession _session;

    public StudentCmd(IDocumentSession session) => _session = session;

    public Student Create(Student student)
    {
        var existing = _session.Load<Student>(student.Id);

        _session.Store(student);
        _session.SaveChanges();

        return existing ?? student;
    }

    public Student? Find(string id)
    {
        return _session.Query<Student>()
            .Search(x => x.Name, "Ravi")
            .Statistics(out var stats)
            .Skip(0)
            .Take(50)
            .FirstOrDefault();
    }
}

public sealed class Student
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}
