using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using AutoBot.Commands;
using AutoBot.Dialogs;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using NLog;
using StepApp.BotExtensions.DialogExtensions;
using StepApp.CommonExtensions.Logger;

namespace AutoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly ILifetimeScope scope;

        private ILoggerService<ILogger> _loggerService;
        private CheckShtrafDialog _checkShtrafDialog;
        private RootDialog _rootDialog;
        private RootLuisDialog _rootLuisDialog;

        public MessagesController()
        {
            // _rootLuisDialog = new RootLuisDialog();
        }

        public MessagesController(ILifetimeScope scope)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
        }

        /// <summary>
        ///     POST: api/Messages
        ///     Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                using (var scope = DialogModule.BeginLifetimeScope(this.scope, activity))
                {
                    _loggerService = scope.Resolve<ILoggerService<ILogger>>();
                    _checkShtrafDialog = scope.Resolve<CheckShtrafDialog>();
                    _rootLuisDialog = scope.Resolve<RootLuisDialog>();
                    _rootDialog = scope.Resolve<RootDialog>();

                    var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                    if (activity.Type == ActivityTypes.Message)
                    {
                        if (activity.Text.Trim() == "reset")
                        {
                            
                            Activity rep;


                            rep = activity.CreateReply("Временные данные успешно удалены");
                            await connector.Conversations.ReplyToActivityAsync(rep);
                            activity.GetStateClient().BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                            return new HttpResponseMessage(HttpStatusCode.Accepted);
                        }

                        if (MessagesCustom.Default.StartSearchFinesCommands.Contains(activity.Text))
                        {
                            //reset stack first
                            var botData = scope.Resolve<IBotData>();
                            await botData.LoadAsync(CancellationToken.None);
                            var _task = scope.Resolve<IDialogTask>();
                            _task.Reset();

                            await Conversation.SendAsync(activity,
                                () => new ExceptionHandlerDialog<object>(_rootDialog, true));

                            return new HttpResponseMessage(HttpStatusCode.Accepted);
                        }

                        if (MessagesCustom.Default.HelpCommands.Contains(activity.Text))
                        {
                            var r = activity.CreateReply("Чтобы начать новый поиск штрафов введите **новый поиск** или нажмите кнопку");
                            var buttonNew = new CardAction
                            {
                                //  Value = "test",
                                Value = "новый поиск",
                                Type = "imBack",
                                Title = "новый поиск"
                            };
                            var cardForButton = new ThumbnailCard { Buttons = new List<CardAction> { buttonNew } };
                            r.Attachments.Add(cardForButton.ToAttachment());
                            await connector.Conversations.ReplyToActivityAsync(r);

                            return new HttpResponseMessage(HttpStatusCode.Accepted);
                        }

                        //tmp
                            // return new HttpResponseMessage(HttpStatusCode.Accepted);
                            //ignore luis now
                            await Conversation.SendAsync(activity,
                            () => new ExceptionHandlerDialog<object>(_rootDialog, true));

                        /* await Conversation.SendAsync(activity,
                                 () => new ExceptionHandlerDialog<object>(_rootLuisDialog, true));*/
                        return new HttpResponseMessage(HttpStatusCode.Accepted);
                        // await Conversation.SendAsync(activity, () => new Dialogs.RootLuisDialog());
                    }

                    else if (activity.Type == ActivityTypes.ConversationUpdate)
                    {

                        //start if just joined
                        if (activity.MembersAdded.Count == 1)
                        {
                           // var act = activity.AsMessageActivity();
                            /* using (var scope = DialogModule.BeginLifetimeScope(this.scope, act))
                             {
                                var _rootDialog = scope.Resolve<RootDialog>();*/



                            await Conversation.SendAsync(activity,
                                () => new ExceptionHandlerDialog<object>(_checkShtrafDialog, true));
                            return new HttpResponseMessage(HttpStatusCode.Accepted);
                            /*}*/
                        }
                        // Handle conversation state changes, like members being added and removed
                        // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                        // Not available in all channels
                    }

                    HandleSystemMessage(activity);
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
            }
            catch (Exception e)
            {
                _loggerService.Error(e);
                throw;
            }
        }

        private async  Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
         /*   else if (message.Type == ActivityTypes.ConversationUpdate)
            {

                //start if just joined
                if (message.MembersAdded.Count == 1)
                {
                    var act = message.AsMessageActivity();
                   /* using (var scope = DialogModule.BeginLifetimeScope(this.scope, act))
                    {
                       var _rootDialog = scope.Resolve<RootDialog>();#1#
                        await Conversation.SendAsync(act,
                            () => new ExceptionHandlerDialog<object>(_rootDialog, true));
                    /*}#1#
                }
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }*/
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}