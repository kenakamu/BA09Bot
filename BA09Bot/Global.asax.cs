using Autofac;
using BA09Bot.Services;
using System.Configuration;
using System.Web.Http;

namespace BA09Bot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static IContainer Container;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AuthBot.Models.AuthSettings.Mode = ConfigurationManager.AppSettings["ActiveDirectory.Mode"];
            AuthBot.Models.AuthSettings.EndpointUrl = ConfigurationManager.AppSettings["ActiveDirectory.EndpointUrl"];
            AuthBot.Models.AuthSettings.Tenant = ConfigurationManager.AppSettings["ActiveDirectory.Tenant"];
            AuthBot.Models.AuthSettings.RedirectUrl = ConfigurationManager.AppSettings["ActiveDirectory.RedirectUrl"];
            AuthBot.Models.AuthSettings.ClientId = ConfigurationManager.AppSettings["ActiveDirectory.ClientId"];
            AuthBot.Models.AuthSettings.ClientSecret = ConfigurationManager.AppSettings["ActiveDirectory.ClientSecret"];

            var builder = new ContainerBuilder();
            builder.RegisterType<D365Service>().As<ID365Service>();
            builder.RegisterType<O365Service>().As<IO365Service>();
            builder.RegisterType<AzureBlobService>().As<IAzureBlobService>();
            builder.RegisterType<ImageService>().As<IImageService>();
            Container = builder.Build();
        }
    }
}
