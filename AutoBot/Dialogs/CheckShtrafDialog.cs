using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using ShrafiBiz.Client;
using ShrafiBiz.Model;

namespace AutoBot.Dialogs
{
    [Serializable]
    public class CheckShtrafDialog : IDialog<object>
    {
        private const int MAX_RETRIES = 3;

        public string sts;
        public string vu;
        public CheckPayResponse pays;
        public string name;
        public string surname;

        public int connectTries;


        public Dictionary<string, Pay> shtrafsAll;
        public Dictionary<string, Pay> shtrafsWantToPay = new Dictionary<string, Pay>();

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"Чтобы проверить штрафы введите СТС");
            context.Wait(ResumeAfterStsEntered);

        }

        private async Task ResumeAfterStsEntered(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var txt = await result;
            sts = txt.Text;
            await context.PostAsync($"Введите ВУ");
            context.Wait(ResumeAfterVuEntered);
            //todo: car recognition by vin or make\model

        }

        private async Task TryCheckPay(IDialogContext context, ShtrafBizClient shtrafiCLient)
        {
            
            pays = shtrafiCLient.CheckPay(sts, vu);

            //нет штрафов
            if (pays.Err == -4)
            {
                await context.PostAsync("Начисления не найдены.");
                context.Done(1);
            }
            //проблемы с сервисом, ретраим
            else if (pays.Err == -1)
            {
                if (connectTries <= MAX_RETRIES)
                {
                    connectTries++;
                    await TryCheckPay(context, shtrafiCLient);
                }
                else
                {
                    await context.PostAsync("Превышено кол-во попыток связи с сервером. Попробуйте воспользоваться позже. Приносим извинения за неудобства.");
                    context.Done(1);
                }   
            }
            else if (pays.Err == 0)
            {
                shtrafsAll = pays.L;
                int i = 1;
                foreach (var pay in pays.L)
                {
                    await context.PostAsync($"{i++}: {pay.Value.ToString()}");
                }
            }


            //  await context.PostAsync($"введите номера штрафов для оплаты или нажмите кнопку \"оплатить все\"");
            // PromptDialog.Text(context, Resume, "введите номера штрафов для оплаты или введите \"все\"", null, 3);
            //  PromptDialog.Choice(context, Resume, new PromptOptions<string>("введите номера штрафов разделяя пробелами или нажмите **оплатить все**", "", null, new List<string>(){ "оплатить все" } ), false);
            // context.Done(1);

            var mes = context.MakeMessage();
            mes.Text = string.Format("введите номера штрафов разделяя пробелами или нажмите **оплатить все**");
            var buttonPay = new CardAction
            {
                //  Value = "test",
                Value = $"оплатить все",
                Type = "imBack",
                Title = "оплатить все"
            };


            var cardForButton = new ThumbnailCard { Buttons = new List<CardAction> { buttonPay } };
            mes.Attachments.Add(cardForButton.ToAttachment());

            await context.PostAsync(mes);
            context.Wait(Resume);
        }

        private async Task ResumeAfterVuEntered(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            var txt = await result;
            vu = txt.Text;
            var shtrafiCLient = new ShtrafBizClient();

            await TryCheckPay(context, shtrafiCLient);





        }

        private async Task Resume(IDialogContext context, IAwaitable<object> result)
        {
            var resu = await result;
            var res = resu as Activity;

            var txt = res.Text;

            if (txt == "оплатить все")
            {
                shtrafsWantToPay = shtrafsAll;
                PromptDialog.Text(context, ResumeAfterShtrafs, "введите фамилию плательщика");
            }
            else
            {
                var shtrafIdsToPay = txt.Split(' ').Select(int.Parse);
                foreach (var id in shtrafIdsToPay)
                {
                    shtrafsWantToPay.Add(shtrafsAll.AsEnumerable().ToList()[id-1].Key, shtrafsAll.AsEnumerable().ToList()[id-1].Value);
                }
                PromptDialog.Text(context, ResumeAfterShtrafs, "введите фамилию плательщика");
                //shtrafsAll
            }
        }

        private async Task ResumeAfterShtrafs(IDialogContext context, IAwaitable<string> result)
        {
            surname = await result;
            PromptDialog.Text(context, ResumeAfterSurname, "введите имя плательщика");
           
        }

        private async Task ResumeAfterSurname(IDialogContext context, IAwaitable<string> result)
        {
            name = await result;
            var shtrafiCLient = new ShtrafBizClient();
            var resp = shtrafiCLient.CreateZkz(shtrafsWantToPay, sts, vu, surname, name);
            if (resp?.Err != -1)
            {
                await context.PostAsync($"Для оплаты перейдите по ссылке: {resp.Urlpay}");
            }
        }
    }
}