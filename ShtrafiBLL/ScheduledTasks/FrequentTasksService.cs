using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BotExtensions.ActivityExtensions;
using BotExtensions.DialogExtensions;
using BotExtensions.Logger;
using BotExtensions.Slack.Etc;
using BusinessLayer.Calendar;
using BusinessLayer.Forms;
using BusinessLayer.GlobalSettings;
using InterFaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Model;
using Model.Entities;
using NLog;
using Quartz;

namespace BusinessLayer.ScheduledTasks
{
    public class FrequentTasksService : IFrequentTasksService
    {
        private readonly ICheckpointService checkpointService;
        private readonly ILoggerService<ILogger> loggerService;
        private IDialog<object> welcomePollDialog;
        private GlobalSettingsService _globalSettingsService;
        // private WelcomePollDialog _welcomePollDialog;
        //     private IDbContext dbContext;



        public FrequentTasksService()
        {
            _globalSettingsService = new GlobalSettings.GlobalSettingsService(new FlowContext());
            loggerService = new LoggerService<ILogger>();
        }

        public FrequentTasksService(/*IDbContext _dbContext,*/ /*ILoggerService<ILogger> _loggerService,*/ ICheckpointService _checkpointService, IDialog<object> _welcomePollDialog)
        {
/*
            //tmp
            welcomePollDialog = _welcomePollDialog;


            checkpointService = _checkpointService;

            //  SetField.NotNull(out dbContext, nameof(dbContext), _dbContext);
            // SetField.NotNull(out loggerService, nameof(loggerService), _loggerService);
            SetField.NotNull(out checkpointService, nameof(checkpointService), _checkpointService);
            // dbContext = _dbContext;

            //  users = dbContext.Set<Model.Entities.User>();*/
        }
        

        public async void Execute(IJobExecutionContext context)
        {
            //  return;
              var chpntService = new CheckpointService(new FlowContext(), new LoggerService<ILogger>());
              var passedChkpoints = chpntService.RegisterPassedCheckpointsForInterval(10);

            //send proactive messages to creators of checkpoint with poll
            if (passedChkpoints != null)
                foreach (var passedCheckpoint in passedChkpoints)
                {
                    await SendFollowupPoll(passedCheckpoint);
                }


            
        }

        private async Task SendFollowupPoll(PassedCheckpoint passedCheckpoint)
        {
            loggerService.Info($"Sending followup poll to {passedCheckpoint.Checkpoint.Creator}");

            var activity = new Activity();

            var botId = _globalSettingsService.BotChannelId;

            activity.From = new ChannelAccount("", "test");
            activity.ServiceUrl = "https://slack.botframework.com/";
            activity.Recipient = new ChannelAccount(botId);
            activity.ChannelId = "slack";


            //log
            /*  loggerService.Info(
                  $"sending proactive message to: {} to ask if chkpnt {} has passed.");*/


            var fakeMessage = await activity.CreateFakeProactiveDirectMessage(passedCheckpoint.Checkpoint.Creator.SlackId);


            var followUpAnswers = new FollowupAfterCheckpointForm(){PassedCheckpoint = passedCheckpoint };


            var myform = new FormDialog<FollowupAfterCheckpointForm>(followUpAnswers, followUpAnswers.BuildForm,
                FormOptions.PromptInStart, null);

            await fakeMessage.StartConversationWithUserFromDialog(new ExceptionHandlerDialog<object>(myform, true));
        }
    }
}
