using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using AutoBot.Dialogs;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;

namespace AutoBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<RootLuisDialog>().AsSelf().InstancePerDependency();

            builder.Update(Conversation.Container);

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
