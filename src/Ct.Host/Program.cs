using Autofac;
using Autofac.Extensions.DependencyInjection;
using NLog.Web;

namespace Ct.Host;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.Host.UseNLog();

        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);
        builder.Host.ConfigureContainer<ContainerBuilder>(startup.ConfigureContainer);

        var app = builder.Build();
        startup.Configure(app, app.Environment);
        app.Run();
    }
}
