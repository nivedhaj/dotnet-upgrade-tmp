using Autofac;
using System.Reflection;

namespace Fees;

public static class FeesModule
{
    public static void Register(ContainerBuilder builder)
    {
        var asm = Assembly.GetExecutingAssembly();

        builder.RegisterAssemblyTypes(asm)
            .Where(t => t.Name.EndsWith("Cmd") || t.Name.EndsWith("Qry") || t.Name.EndsWith("Service"))
            .AsSelf();
    }
}
