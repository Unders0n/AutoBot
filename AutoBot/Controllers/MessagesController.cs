using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoBot.Dialogs;
using Autofac;
using BotExtensions.DialogExtensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace AutoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private RootLuisDialog _rootLuisDialog;

        private readonly ILifetimeScope scope;

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
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            using (var scope = DialogModule.BeginLifetimeScope(this.scope, activity))
            {
                _rootLuisDialog = scope.Resolve<RootLuisDialog>();
            }


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
                await Conversation.SendAsync(activity,
                            () => new ExceptionHandlerDialog<object>(_rootLuisDialog, true));
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