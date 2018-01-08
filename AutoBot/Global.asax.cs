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
using Quartz;
using Quartz.Impl;
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


                //sheduled tasks
                builder.Register(x => new StdSchedulerFactory().GetScheduler()).As<IScheduler>();

                RegisterRecurrentTasks();

                logger.Info("recurrent tasks started...");

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

        private void RegisterRecurrentTasks()
        {
            //enabling scheduled tasks via Quartz.NET 
            try
            {
                //  Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info };

                // Grab the Scheduler instance from the Factory 
                var scheduler = Conversation.Container.Resolve<IScheduler>();
                //  IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

                //   scheduler.JobFactory = new CommonExtensions.ScheduledTasks.Quartz.AutofacJobFactory(Conversation.Container);

                var jobDetail = new JobDetailImpl("CheckShtrafs", "group1", typeof(FrequentTasksService));

                scheduler.Start();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(10)
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
