using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoBot.Commands;
using Autofac;
using StepApp.BotExtensions.DialogExtensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using ShtrafiBLL;
using StepApp.CommonExtensions.Assembly;

namespace AutoBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {

        private  WelcomeAndRegisterCarDialog welcomeAndRegisterDialog;
        private CheckShtrafDialog _checkShtrafDialog;

         public RootDialog(CheckShtrafDialog checkShtrafDialog)
        {
            SetField.NotNull(out _checkShtrafDialog, nameof(_checkShtrafDialog), checkShtrafDialog);
        }

        public async Task StartAsync(IDialogContext context)
        {
          //  welcomeAndRegisterDialog = new WelcomeAndRegisterCarDialog();
           // checkShtrafDialog = new CheckShtrafDialog();
            context.Wait(MessageReceivedAsync);

           // return Task.CompletedTask;


        }

        //refactor to scorables
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            
            
            
            if (MessagesCustom.Default.StartSearchFinesCommands.Contains(activity.Text))
            {
                await context.Forward(new ExceptionHandlerDialog<object>(_checkShtrafDialog, true),
                    AfterWelcomeAndRegisterDialog,
                    "",
                    CancellationToken.None);
                return;
            }

            /*if (activity.Text.Contains("/start"))
            {
                await context.Forward(new ExceptionHandlerDialog<object>(_checkShtrafDialog, true),
                    AfterWelcomeAndRegisterDialog,
                    "",
                    CancellationToken.None);
                return;
            }
            if (activity.Text.Contains("искать ещё"))
            {
                await context.Forward(new ExceptionHandlerDialog<object>(_checkShtrafDialog, true),
                    AfterWelcomeAndRegisterDialog,
                    "",
                    CancellationToken.None);
            }*/

            if (activity.Text.Trim() == "/version")
            {
                var msg = string.Format("Автобот. {0} ({1}). Развернут на {2}", AssemblyHelper.GetAssemblyVersionDate(),
                    AssemblyHelper.GetAssemblyVersion(),
                    AssemblyHelper.GetAppHostUri());

                await context.PostAsync(msg);
                return;
            }


            context.Wait(MessageReceivedAsync);
        }

        private async Task AfterWelcomeAndRegisterDialog(IDialogContext context, IAwaitable<object> result)
        {
            //  context.Done(1);
            var button = new CardAction
            {
                //  Value = "test",
                Value = $"искать ещё",
                Type = "imBack",
                Title = "искать ещё"
            };
            await context.PostWithButtonsAsync("доступные опции:", new List<CardAction>() { button });
            context.Wait(MessageReceivedAsync);
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