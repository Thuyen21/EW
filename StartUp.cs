using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebApplication1.App_Start.StartUp))]
namespace WebApplication1.App_Start
{
    public partial class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

    }

}