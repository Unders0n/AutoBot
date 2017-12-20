using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using AutoBot.Dialogs;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Model;
using NLog;
using ShtrafiBLL;
using StepApp.CommonExtensions.Logger;

namespace AutoBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var logger = new LoggerService<ILogger>();
            logger.Info("Starting service...");

            try
            {
                var builder = new ContainerBuilder();

                //register dialogs
                builder.RegisterType<RootLuisDialog>().AsSelf().InstancePerDependency();
                builder.RegisterType<RootDialog>().AsSelf().InstancePerDependency();
                builder.RegisterType<WelcomeAndRegisterCarDialog>().AsSelf().InstancePerDependency();
                builder.RegisterType<CheckShtrafDialog>().AsSelf().InstancePerDependency();


                //register dbcontext
                builder.RegisterType<AutoBotContext>()
                   .Keyed<AutoBotContext>(FiberModule.Key_DoNotSerialize).AsSelf()
                   .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

                //register business services
                builder.RegisterType<ShtrafiUserService>()
                        .Keyed<IShtrafiUserService>(FiberModule.Key_DoNotSerialize)
                        .AsImplementedInterfaces()
                        .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

                builder.RegisterGeneric(typeof(LoggerService<>)).As(typeof(ILoggerService<>)).InstancePerDependency();

                var config = GlobalConfiguration.Configuration;

                // Register your Web API controllers.
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                // OPTIONAL: Register the Autofac filter provider.
                builder.RegisterWebApiFilterProvider(config);

                /* builder.RegisterType<MessagesController>().InstancePerDependency();
                 builder.RegisterType<InteractiveMenuController>().InstancePerDependency();*/

                GlobalConfiguration.Configuration.DependencyResolver =
                    new AutofacWebApiDependencyResolver(Conversation.Container);

                builder.Update(Conversation.Container);


                GlobalConfiguration.Configure(WebApiConfig.Register);


                logger.Info("Service successfully started");

                //  GlobalConfiguration.Configure(WebApiConfig.Register);
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
           
        }
    }
}
