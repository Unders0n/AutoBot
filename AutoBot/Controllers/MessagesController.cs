using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
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
        private RootLuisDialog _rootLuisDialog;
        private RootDialog _rootDialog;

        private readonly ILifetimeScope scope;

        private ILoggerService<ILogger> _loggerService;

        public MessagesController()
        {
           // _rootLuisDialog = new RootLuisDialog();
        }

        public MessagesController(ILifetimeScope scope)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken token)
        {
            try
            {
                using (var scope = DialogModule.BeginLifetimeScope(this.scope, activity))
                {
                    _loggerService = scope.Resolve<ILoggerService<ILogger>>();
                    _rootLuisDialog = scope.Resolve<RootLuisDialog>();
                    _rootDialog = scope.Resolve<RootDialog>();
                   

                    if (activity.Type == ActivityTypes.Message)
                    {
                        if (activity.Text.Trim() == "reset")
                        {
                            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            Activity rep;


                            rep = activity.CreateReply("Временные данные успешно удалены");
                            await connector.Conversations.ReplyToActivityAsync(rep);
                            activity.GetStateClient().BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                            return new HttpResponseMessage(HttpStatusCode.Accepted);
                        }
                        //ignore luis now
                        await Conversation.SendAsync(activity,
                            () => new ExceptionHandlerDialog<object>(_rootDialog, true), token);

                        /* await Conversation.SendAsync(activity,
                             () => new ExceptionHandlerDialog<object>(_rootLuisDialog, true));*/
                        return new HttpResponseMessage(HttpStatusCode.Accepted);
                        // await Conversation.SendAsync(activity, () => new Dialogs.RootLuisDialog());
                    }
                    else
                    {
                        HandleSystemMessage(activity);
                    }
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

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
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