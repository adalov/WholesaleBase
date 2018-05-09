using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WholesaleBase.Startup))]
namespace WholesaleBase
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
