using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoBot.Dialogs;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using NLog;
using Quartz;
using ShrafiBiz.Client;
using StepApp.CommonExtensions.Logger;

namespace AutoBot.ScheduledTasks
{
    public class ScheduledShtrafsCheckService : IScheduledShtrafsCheckService
    {
        private readonly IShtrafBizClient _shtrafiBizClient;

        private readonly ILoggerService<ILogger> loggerService;
        private IDialog<object> welcomePollDialog;

        private List<UsersShtrafiWithDocSet> allShtrafs = new List<UsersShtrafiWithDocSet>();


        private const int MAX_RETRIES = 3;
        public int connectTries;
        private CheckShtrafDialog _dialogToStart;

        // private GlobalSettingsService _globalSettingsService;
        // private WelcomePollDialog _welcomePollDialog;
        //     private IDbContext dbContext;

        public ScheduledShtrafsCheckService()
        {
            _shtrafiBizClient = Conversation.Container.Resolve<IShtrafBizClient>();
        }

        //todo: resolve issue with DI
        public ScheduledShtrafsCheckService(IShtrafBizClient shtrafiBizClient, CheckShtrafDialog dialogToStart)
        {
            _dialogToStart = dialogToStart;
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
            await SendNewShtrafNotification(allShtrafs);
        }

        private async Task SendNewShtrafNotification(List<UsersShtrafiWithDocSet> allShtrafs)
        {
            loggerService.Info("Sending proactive info about shtrafs");

            

            foreach (var shtrafiWithDocSet in allShtrafs)
            {
                loggerService.Info($"Sending proactive info about {shtrafiWithDocSet.DocumentSetToCheck}");
                // var _dialogToStart = Conversation.Container.Resolve<CheckShtrafDialog>();
                _dialogToStart.ShtrafsToShow = shtrafiWithDocSet.Shtrafs;
                await SendProactive(shtrafiWithDocSet.User.MainConversationReferenceSerialized, _dialogToStart);
                loggerService.Info($"proactive info about {shtrafiWithDocSet.DocumentSetToCheck} successful");
            }
        }

        public async Task SendProactive(string conversationReference, IDialog<object> dialogToStart)
        {
            // Recreate the message from the conversation reference that was saved previously.
            var message = JsonConvert.DeserializeObject<ConversationReference>(conversationReference).GetPostToBotMessage();
            var client = new ConnectorClient(new Uri(message.ServiceUrl));

            // Create a scope that can be used to work with state from bot framework.
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
            {
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync(CancellationToken.None);

                // This is the dialog stack.
                var stack = scope.Resolve<IDialogTask>();

                // Create the new dialog and add it to the stack.
                
                stack.Call(dialogToStart.Void<object, IMessageActivity>(), null);
                await stack.PollAsync(CancellationToken.None);

                // Flush the dialog stack back to its state store.
                await botData.FlushAsync(CancellationToken.None);
            }
        }


      /*  private static async Task SendProactive(IDialog<object> dialog, string userSlackId, string userSlackName, object[] customParams = null)
        {
            var activity = new Microsoft.Bot.Connector.Activity();

            // var botId = _globalSettingsService.BotChannelId;
            var botId = "B7ZEWQX0V:T7XPDKESV";

            activity.From = new ChannelAccount("", "test");
            activity.ServiceUrl = "https://slack.botframework.com/";
            activity.Recipient = new ChannelAccount(botId);
            activity.ChannelId = "slack";

            var fakeMessage = await activity.CreateFakeProactiveDirectMessage(userSlackId, userSlackName);

            //try to paste phrase


            await fakeMessage.StartConversationWithUserFromDialog(new ExceptionHandlerDialog<object>(dialog, true), null, customParams);
        }*/


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