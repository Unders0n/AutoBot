using System;
using System.Threading;
using System.Threading.Tasks;
using StepApp.BotExtensions.DialogExtensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace AutoBot.Dialogs
{
    [Serializable]
    [LuisModel("e602ed8f-2174-45c0-bc30-f09e84712e70", "35f95c56928740d5b7ba97185473188e", LuisApiVersion.V2)]
    public class RootLuisDialog : LuisDialog<IMessageActivity>
    {
        private WelcomeAndRegisterCarDialog _welcomeAndRegisterCarDialog;

        public RootLuisDialog(WelcomeAndRegisterCarDialog welcomeAndRegisterCarDialog)
        {
            SetField.NotNull(out _welcomeAndRegisterCarDialog, nameof(_welcomeAndRegisterCarDialog), welcomeAndRegisterCarDialog);
            //   welcomeAndRegisterDialog = new WelcomeAndRegisterCarDialog();

        }

        [LuisIntent("CarService")]
        public async Task CarService(IDialogContext context, LuisResult result)
        {
            //test
            // await context.PostAsync(Messages.Default.NothingToDoNowMessages.GetRandomString());
        }

        [LuisIntent("")]
        public async Task Nothing(IDialogContext context, LuisResult result)
        {
           // await context.PostAsync(Messages.Default.NothingToDoNowMessages.GetRandomString());
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            var activity = context.Activity as IMessageActivity;

            //1st time
            if (activity.Text.Contains("/start"))
            {

                await context.Forward(new ExceptionHandlerDialog<object>(_welcomeAndRegisterCarDialog, true),
                   AfterWelcomeAndRegisterDialog,
                   "",
                   CancellationToken.None);
                return;
            }

           // context.Wait(MessageReceivedAsync);
        }

        private async Task AfterWelcomeAndRegisterDialog(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync("Отлично. Теперь можно воспользоваться нашими бесплатными сервисами, например введите 'ТО' чтобы получить информацию о последующем техническом обслуживании");
            await ShowMenu(context);
            //  throw new NotImplementedException();
        }

        private async Task ShowMenu(IDialogContext context)
        {

        }
    }
}