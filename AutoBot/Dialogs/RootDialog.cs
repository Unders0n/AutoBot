using System;
using System.Threading;
using System.Threading.Tasks;
using BotExtensions.DialogExtensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace AutoBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {

        private  WelcomeAndRegisterCarDialog welcomeAndRegisterDialog;
        private CheckShtrafDialog checkShtrafDialog;

        public Task StartAsync(IDialogContext context)
        {
          //  welcomeAndRegisterDialog = new WelcomeAndRegisterCarDialog();
            checkShtrafDialog = new CheckShtrafDialog();
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            //1st time
            if (activity.Text.Contains("/start"))
            {
                
                await context.Forward(new ExceptionHandlerDialog<object>(checkShtrafDialog, true),
                   AfterWelcomeAndRegisterDialog,
                   "",
                   CancellationToken.None);
                return;
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task AfterWelcomeAndRegisterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done(1);
          /*  await context.PostAsync("Отлично. Теперь можно воспользоваться нашими бесплатными сервисами, например введите 'ТО' чтобы получить информацию о последующем техническом обслуживании");
            await ShowMenu(context);*/
            //  throw new NotImplementedException();
        }

        private async Task ShowMenu(IDialogContext context)
        {
            //test
        }
    }
}