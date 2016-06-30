using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OnTrace.Channel.WebUI.Startup))]
namespace OnTrace.Channel.WebUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
