using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FingerprintManagementConsole.Startup))]
namespace FingerprintManagementConsole
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
