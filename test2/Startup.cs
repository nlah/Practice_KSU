using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(test2.Startup))]
namespace test2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
