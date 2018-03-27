using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using StepApp.BotExtensions.ActivityExtensions;
using StepApp.BotExtensions.DialogExtensions;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Model;
using Model.Entities;
using Model.Entities.Fines;
using NLog;
using Quartz;
using ShrafiBiz.Client;
using ShrafiBiz.Model;
using StepApp.CommonExtensions.Logger;

namespace BusinessLayer.ScheduledTasks
{
    public class ScheduledShtrafsCheckService : IScheduledShtrafsCheckService
    {
        private readonly IShtrafBizClient _shtrafiBizClient;

        private readonly ILoggerService<ILogger> loggerService;
        private IDialog<object> welcomePollDialog;

        private List<UsersShtrafiWithDocSet> allShtrafs = new List<UsersShtrafiWithDocSet>();


        private const int MAX_RETRIES = 3;
        public int connectTries;

        // private GlobalSettingsService _globalSettingsService;
        // private WelcomePollDialog _welcomePollDialog;
        //     private IDbContext dbContext;

        public ScheduledShtrafsCheckService()
        {
            _shtrafiBizClient = Conversation.Container.Resolve<IShtrafBizClient>();
        }

        //todo: resolve issue with DI
        public ScheduledShtrafsCheckService(IShtrafBizClient shtrafiBizClient)
        {
            _shtrafiBizClient = shtrafiBizClient;
            // _globalSettingsService = new GlobalSettings.GlobalSettingsService(new FlowContext());
            loggerService = new LoggerService<ILogger>();
        }

        /*  public FrequentTasksService(  IDialog<object> _welcomePollDialog)
          {
  /*
              //tmp
              welcomePollDialog = _welcomePollDialog;
  
  
              checkpointService = _checkpointService;
  
              //  SetField.NotNull(out dbContext, nameof(dbContext), _dbContext);
              // SetField.NotNull(out loggerService, nameof(loggerService), _loggerService);
              SetField.NotNull(out checkpointService, nameof(checkpointService), _checkpointService);
              // dbContext = _dbContext;
  
              //  users = dbContext.Set<Model.Entities.User>();#1#
          }*/


        public async Task Execute(IJobExecutionContext context)
        {

            var dbContext = new Model.AutoBotContext();

            //check who need to be checked
            var docSets = dbContext.DocumentSetsTocheck.Where(check => check.ScheduleCheck).Include(check => check.User).ToList();

            //todo: add queue and splitting by stack (or sending by 10 batch), for now just all 1by1
            foreach (var docSet in docSets)
            {
                connectTries = 0;
                await TryCheckPay();

                async Task TryCheckPay()
                {
                    var pays = _shtrafiBizClient.CheckPay(docSet.Sts, docSet.Vu);
                    if (pays.Err == -1)
                    {
                        if (connectTries <= MAX_RETRIES)
                        {
                            connectTries++;
                            await TryCheckPay();
                        }
                        else
                        {
                            loggerService.Error("Превышено кол-во попыток подключения к сервису.");
                        }
                    }
                    else if (pays.Err == 0)
                    {
                        var userShtrafs = pays.L;
                    //    allShtrafs.Add(new Tuple<User, DocumentSetToCheck, System.Collections.Generic.Dictionary<string,Pay>>(docSet.User, docSet, userShtrafs));
                        allShtrafs.Add(new UsersShtrafiWithDocSet(docSet.User, docSet, userShtrafs));

                    }


                }



            }
        }

        private async Task SendFollowupPoll()
            {
                /* loggerService.Info($"Sending followup poll to {passedCheckpoint.Checkpoint.Creator}");
     
                 var activity = new Activity();
     
                 var botId = _globalSettingsService.BotChannelId;
     
                 activity.From = new ChannelAccount("", "test");
                 activity.ServiceUrl = "https://slack.botframework.com/";
                 activity.Recipient = new ChannelAccount(botId);
                 activity.ChannelId = "slack";
     
     
                 //log
                 /*  loggerService.Info(
                       $"sending proactive message to: {} to ask if chkpnt {} has passed.");#1#
     
     
                 var fakeMessage = await activity.CreateFakeProactiveDirectMessage(passedCheckpoint.Checkpoint.Creator.SlackId);
     
     
                 var followUpAnswers = new FollowupAfterCheckpointForm(){PassedCheckpoint = passedCheckpoint };
     
     
                 var myform = new FormDialog<FollowupAfterCheckpointForm>(followUpAnswers, followUpAnswers.BuildForm,
                     FormOptions.PromptInStart, null);
     
                 await fakeMessage.StartConversationWithUserFromDialog(new ExceptionHandlerDialog<object>(myform, true));*/
            }
        }


    }

public class UsersShtrafiWithDocSet
{
    public UsersShtrafiWithDocSet(User user, DocumentSetToCheck documentSetToCheck, Dictionary<string, Pay> shtrafs)
    {
        User = user;
        DocumentSetToCheck = documentSetToCheck;
        Shtrafs = shtrafs;
    }

    public User User { get; set; }
    public DocumentSetToCheck DocumentSetToCheck { get; set; }
    public Dictionary<string, Pay> Shtrafs { get; set; }
}


