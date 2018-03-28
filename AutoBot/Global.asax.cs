using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
using AutoBot.Dialogs;
using Autofac;
using Autofac.Integration.WebApi;
using BusinessLayer.ScheduledTasks;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Model;
using NLog;
using Quartz;
using Quartz.Impl;
using ShrafiBiz.Client;
using ShtrafiBLL;
using StepApp.CommonExtensions.Logger;
using StepApp.CommonExtensions.ScheduledTasks.Quartz;

namespace AutoBot
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            var config = GlobalConfiguration.Configuration;

            Conversation.UpdateContainer(
                builder =>
                {
                    //register dialogs
                    builder.RegisterType<RootLuisDialog>().AsSelf().InstancePerDependency();


                    //  builder.RegisterType<RootDialog>().AsSelf().InstancePerDependency();
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

                    builder.RegisterType<ShtrafBizClient>()
                        .Keyed<IShtrafBizClient>(FiberModule.Key_DoNotSerialize)
                        .AsImplementedInterfaces()
                        .SingleInstance();
                    //  .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);


                    builder.RegisterGeneric(typeof(LoggerService<>)).As(typeof(ILoggerService<>))
                        .InstancePerDependency();

                    // Bot Storage: Here we register the state storage for your bot. 
                    // Default store: volatile in-memory store - Only for prototyping!

                    //BOT Data storage

                    builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                    var store = new InMemoryDataStore();

                    builder.Register(c => store)
                        .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                        .AsSelf()
                        .SingleInstance();

                    // Register your Web API controllers.
                    builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                    builder.RegisterWebApiFilterProvider(config);

                    //sheduled tasks
                    builder.Register(x => new StdSchedulerFactory().GetScheduler().Result).As<IScheduler>();


                    builder.Register(
                            (c, p) =>
                                new ScheduledShtrafsCheckService(c.Resolve<IShtrafBizClient>(), c.Resolve<CheckShtrafDialog>()))
                        .AsSelf().SingleInstance();
                    // .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);
                });


            // Set the dependency resolver to be Autofac.
            config.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);

            // WebApiConfig stuff
            GlobalConfiguration.Configure(cfg =>
            {
                cfg.MapHttpAttributeRoutes();

                cfg.Routes.MapHttpRoute(
                    "DefaultApi",
                    "api/{controller}/{id}",
                    new {id = RouteParameter.Optional}
                );
            });

            RegisterRecurrentTasks();
        }
/*

        protected void Application_Start2()
        {
            var logger = new LoggerService<ILogger>();
            logger.Info("Starting service...");

            try
            {
                var builder = new ContainerBuilder();

                //register dialogs
                builder.RegisterType<RootLuisDialog>().AsSelf().InstancePerDependency();


                //  builder.RegisterType<RootDialog>().AsSelf().InstancePerDependency();
                builder.RegisterType<RootDialog>().As<IDialog<object>>().InstancePerDependency();


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

                builder.RegisterType<IShtrafBizClient>()
                    .Keyed<IShtrafBizClient>(FiberModule.Key_DoNotSerialize)
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);


                builder.RegisterGeneric(typeof(LoggerService<>)).As(typeof(ILoggerService<>)).InstancePerDependency();

                var config = GlobalConfiguration.Configuration;

                // Register your Web API controllers.
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                // OPTIONAL: Register the Autofac filter provider.
                builder.RegisterWebApiFilterProvider(config);

                /* builder.RegisterType<MessagesController>().InstancePerDependency();
                 builder.RegisterType<InteractiveMenuController>().InstancePerDependency();#1#

                /*GlobalConfiguration.Configuration.DependencyResolver =
                    new AutofacWebApiDependencyResolver(Conversation.Container);#1#


                //sheduled tasks
                builder.Register(x => new StdSchedulerFactory().GetScheduler().Result).As<IScheduler>();


                builder.Register(
                        (c, p) =>
                            new ScheduledShtrafsCheckService(c.Resolve<IShtrafBizClient>()))
                    .AsSelf()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);


                //   builder.Build();
                var container = builder.Build();
                GlobalConfiguration.Configuration.DependencyResolver =
                    new AutofacWebApiDependencyResolver(Conversation.Container);

                //   DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

                //   builder.Update(Conversation.Container);


                GlobalConfiguration.Configure(WebApiConfig.Register);

                logger.Info("starting recurrent tasks...");

                //   RegisterRecurrentTasks();

                logger.Info("recurrent tasks started...");


                logger.Info("Service successfully started");

                //  GlobalConfiguration.Configure(WebApiConfig.Register);
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }
*/

        private void RegisterRecurrentTasks()
        {
            //enabling scheduled tasks via Quartz.NET 
            try
            {
                //  Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info };

                // Grab the Scheduler instance from the Factory 
                var scheduler = Conversation.Container.Resolve<IScheduler>();
                //  var scheduler = container.Resolve<IScheduler>();
                //  IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

                scheduler.JobFactory = new AutofacJobFactory(Conversation.Container);

                var jobDetail = new JobDetailImpl("CheckShtrafs", "group1", typeof(ScheduledShtrafsCheckService));

                scheduler.Start();


                //todo: change time
                var trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(24)
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                scheduler.ScheduleJob(jobDetail, trigger);
            }
            catch (SchedulerException se)
            {
                var logger = new LoggerService<ILogger>();
                logger.Error(se);
            }
        }
    }
}