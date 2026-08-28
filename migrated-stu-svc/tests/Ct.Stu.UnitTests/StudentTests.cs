using Xunit; using Ct.Modules.Academics.Domain;
namespace Ct.Modules.Academics.Tests;
public sealed class StudentTests { [Fact] public void NewStudent_IsActive(){var s=new Student();Assert.Equal(StudentStatus.Active,s.Status);Assert.NotEqual(Guid.Empty,s.Id); } }
