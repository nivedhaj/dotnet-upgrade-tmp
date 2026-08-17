using Autofac;
using ct._Core.Data;
using ct._Core.Security;
using System.Reflection;

namespace Student;

public static class StudentModule
{
    public static void Register(ContainerBuilder builder)
    {
        var asm = Assembly.GetExecutingAssembly();

        builder.RegisterAssemblyTypes(asm)
            .Where(t => t.Name.EndsWith("Cmd") || t.Name.EndsWith("Qry") || t.Name.EndsWith("Service"))
            .WithParameter(new ResolvedParameter(
                (pi, _) => pi.ParameterType == typeof(IUserContext),
                (_, ctx) => ctx.Resolve<IUserContext>()))
            .AsSelf();
    }
}
