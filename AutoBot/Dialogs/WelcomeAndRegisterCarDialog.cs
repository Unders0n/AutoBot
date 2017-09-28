using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace AutoBot.Dialogs
{
    [Serializable]
    public class WelcomeAndRegisterCarDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"Привет, {context.Activity.From.Name}. я АвтоБот и могу помогать тебе с различными вопросами по твоему авто. Мы берем информацию по авто из актуальных баз производителей, а всю правовую информацию из новейших баз официальных источников министерств РФ.");
            await context.PostAsync($"У нас нет утомительной регистрации, но я хотел бы узнать о твоей машине чтобы знать чем я могу помочь.");
            await context.PostAsync($"Введи марку своего авто. Можно вводить в свободной форме либо ввести VIN код.");

            context.Wait(ResumeAfterCarInfoEntered);

        }

        private async Task ResumeAfterCarInfoEntered(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var txt = await result;
            //todo: car recognition by vin or make\model
            PromptDialog.Confirm(context, ResumeAfterConfirmCar, $"Вы ввели {txt.Text}. Мы нашли машину **Volkswagen Golf 6 1.4 tsi 5 дв** Это верно?");
          
        }

        private async Task ResumeAfterConfirmCar(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                //car recognised
                PromptDialog.Number(context, ResumeAfterYear, $"Чтобы напоминать вам о сервисных работах, позвольте узнать год выпуска вашего авто");
                

            }
            else
            {
                await context.PostAsync($"Попробуй ещё раз. Введи марку своего авто. Можно вводить в свободной форме либо ввести VIN код.");
                context.Wait(ResumeAfterCarInfoEntered);
            }
        }

        private async Task ResumeAfterYear(IDialogContext context, IAwaitable<double> result)
        {
            var year = await result;
            PromptDialog.Number(context, ResumeAfterMileage, $"{year}. Ок. ...И пробег");
        }

        private async Task ResumeAfterMileage(IDialogContext context, IAwaitable<long> result)
        {
            var mileage = await result;
            
            context.Done(1);
        }
    }
}