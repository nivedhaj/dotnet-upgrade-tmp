using Xunit; using Ct.Stu.Domain;
namespace Ct.Stu.UnitTests;
public sealed class StudentTests { [Fact] public void NewStudent_IsActive(){var s=new Student();Assert.Equal(StudentStatus.Active,s.Status);Assert.NotEqual(Guid.Empty,s.Id); } }
