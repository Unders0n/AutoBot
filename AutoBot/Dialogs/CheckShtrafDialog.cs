using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.ConnectorEx;
using StepApp.BotExtensions.DialogExtensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Model.Entities;
using Newtonsoft.Json;
using ShrafiBiz.Client;
using ShrafiBiz.Model;
using ShtrafiBLL;

namespace AutoBot.Dialogs
{
    [Serializable]
    public class CheckShtrafDialog : IDialog<object>
    {
        private const int MAX_RETRIES = 3;

        private const int MAX_FINES_TO_SHOW = 10;

        public int connectTries;
        public string name;
        public CheckPayResponse pays;


        public Dictionary<string, Pay> shtrafsAll;
        public Dictionary<string, Pay> shtrafsWantToPay = new Dictionary<string, Pay>();

        public string sts;
        public string surname;
        public string vu;
        private IShtrafiUserService _shtrafiUserService;
        private User user;

        public CheckShtrafDialog(IShtrafiUserService shtrafiUserService)
        {
            SetField.NotNull(out _shtrafiUserService, nameof(_shtrafiUserService), shtrafiUserService);
        }

       /* public CheckShtrafDialog()
        {

        }*/

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"Чтобы проверить штрафы, введите номер свидетельства о регистрации ТС");
            context.Wait(ResumeAfterStsEntered);
        }

        private async Task ResumeAfterStsEntered(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            
           
            var txt = await result;
            sts = txt.Text;
            //validation
            if (sts.Length != 10)
            {
                await context.PostAsync($"Неверный формат ввода, убедитесь что вводите 10 символов номера.");
                context.Wait(ResumeAfterStsEntered);
                return;
            }
            //  await context.PostAsync($"Введите номер водительского удостоверения. (Шаг можно пропустить, хотя наличие ВУ повышает шанс на поиск штрафа)");

            /*var mes = context.MakeMessage();
            mes.Text =
                "Введите номер водительского удостоверения. (Шаг можно пропустить, хотя наличие ВУ повышает шанс на поиск штрафа)";
            var buttonPay = new CardAction
            {
                //  Value = "test",
                Value = $"пропустить",
                Type = "imBack",
                Title = "пропустить"
            };


            var cardForButton = new ThumbnailCard {Buttons = new List<CardAction> {buttonPay}};
            mes.Attachments.Add(cardForButton.ToAttachment());

            await context.PostAsync(mes);*/
            var buttonPay = new CardAction
            {
                //  Value = "test",
                Value = $"пропустить",
                Type = "imBack",
                Title = "пропустить"
            };
            await context.PostWithButtonsAsync("Введите номер водительского удостоверения. (Шаг можно пропустить, хотя наличие ВУ повышает шанс на поиск штрафа)", new List<CardAction>(){buttonPay});



            context.Wait(ResumeAfterVuEntered);
            //todo: car recognition by vin or make\model
        }

        private async Task ResumeAfterVuEntered(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var txt = await result;
            if (txt.Text == "пропустить")
                vu = "";
            else
            {
                vu = txt.Text;
                //validation
                if (vu.Length != 10)
                {
                    await context.PostAsync($"Неверный формат ввода, убедитесь что вводите 10 символов номера.");
                    context.Wait(ResumeAfterVuEntered);
                    return;
                }
            }

            var shtrafiCLient = new ShtrafBizClient();

            await TryCheckPay(context, shtrafiCLient);
        }

        private async Task TryCheckPay(IDialogContext context, ShtrafBizClient shtrafiCLient)
        {
            pays = shtrafiCLient.CheckPay(sts, vu);

            //register user if not
            //todo: separate into 2 methods to decrease load
            var conversationReference = context.Activity.ToConversationReference();
            var dialogRefSerialized = JsonConvert.SerializeObject(conversationReference);

            user = _shtrafiUserService.GetUserAndRegisterIfNeeded(context.Activity.From.Id, dialogRefSerialized);

            


            //нет штрафов
            if (pays.Err == -4)
            {
                await context.PostAsync("**Штрафов не найдено.**");
                await Subscribe(context);
               // context.Done(1);
                return;
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
                    await context.PostAsync(
                        "Превышено кол-во попыток связи с сервером. Попробуйте воспользоваться позже. Приносим извинения за неудобства.");
                    context.Done(1);
                }
            }
            else if (pays.Err == 15)
            {
                await context.PostAsync(
                    "Неверно введен один из документов для проверки.");
                await context.PostAsync($"Чтобы проверить штрафы, введите номер свидетельства о регистрации ТС");
                context.Wait(ResumeAfterStsEntered);

            }
            else if (pays.Err == 0)
            {
                shtrafsAll = pays.L;
                var i = 1;
                string shtrafiText = "";
                
                foreach (var pay in pays.L)
                {
                    // await context.PostAsync($"{i++}: {pay.Value}");
                    shtrafiText += $"**{i++}**: {pay.Value} <br/><br/> ";
                    //limit nmb of fines
                    if (i == MAX_FINES_TO_SHOW + 1)
                    {
                        shtrafiText +=
                            $"Показано {MAX_FINES_TO_SHOW} штрафов из {pays.L.Count}. Оплатите данные штрафы и повторите поиск";
                        break;
                    }
                }

                await context.PostAsync(shtrafiText);

                var totalSumm = pays.L.Take(MAX_FINES_TO_SHOW).Sum(pair => Int16.Parse(pair.Value.Sum));
                var totalSummFeesrv = pays.L.Take(MAX_FINES_TO_SHOW).Sum(pair => pair.Value.Feesrv);
                await context.PostAsync(
                    $"Всего **{pays.L.Take(MAX_FINES_TO_SHOW).ToList().Count}** штрафов на общую сумму **{totalSumm}**р (+ {totalSummFeesrv}р комиссия)");


                //  await context.PostAsync($"введите номера штрафов для оплаты или нажмите кнопку \"оплатить все\"");
                // PromptDialog.Text(context, Resume, "введите номера штрафов для оплаты или введите \"все\"", null, 3);
                //  PromptDialog.Choice(context, Resume, new PromptOptions<string>("введите номера штрафов разделяя пробелами или нажмите **оплатить все**", "", null, new List<string>(){ "оплатить все" } ), false);
                // context.Done(1);

                var mes = context.MakeMessage();
                mes.Text = "введите номера штрафов, разделяя пробелами, или нажмите **оплатить все**";
                var buttonPay = new CardAction
                {
                    //  Value = "test",
                    Value = $"оплатить все",
                    Type = "imBack",
                    Title = "оплатить все"
                };

                var buttonNew = new CardAction
                {
                    //  Value = "test",
                    Value = "новый",
                    Type = "imBack",
                    Title = "новый поиск"
                };


                var cardForButton = new ThumbnailCard { Buttons = new List<CardAction> { buttonPay, buttonNew } };
                mes.Attachments.Add(cardForButton.ToAttachment());

                await context.PostAsync(mes);
                context.Wait(AfterSelectShtrafsToPay);

            }


        }

        

        private async Task AfterSelectShtrafsToPay(IDialogContext context, IAwaitable<object> result)
        {
            var resu = await result;
            var res = resu as Activity;

            var txt = res.Text;

            if (res.Text == "новый")
            {
                if (_shtrafiUserService.GetDocumentSetToCheck(user, sts) == null)
                {
                    PromptDialog.Confirm(context, ResumeAfterAskToSaveSubscribtion,
                        "Хотите получать уведомления о новых штрафах автоматически?");
                    return;
                }

                context.Done(1);
                return;
                //todo: add cancelling of subscription
            }

            if (txt == "оплатить все")
            {
                shtrafsWantToPay = shtrafsAll;
            }
            else
            {
                var shtrafIdsToPay = txt.Split(' ').Select(int.Parse);
                foreach (var id in shtrafIdsToPay)
                    shtrafsWantToPay.Add(shtrafsAll.AsEnumerable().ToList()[id - 1].Key,
                        shtrafsAll.AsEnumerable().ToList()[id - 1].Value);
                //shtrafsAll
            }

            var totalSumm = shtrafsWantToPay.Sum(pair => Int16.Parse(pair.Value.Sum));
            var totalSummFeesrv = shtrafsWantToPay.Sum(pair => pair.Value.Feesrv);

            await context.PostAsync(
                $"Выбрано штрафов: **{shtrafsWantToPay.Count}**, на общую сумму **{totalSumm + totalSummFeesrv}**р (из них комиссия {totalSummFeesrv}р) ");
            PromptDialog.Text(context, ResumeAfterSurname, "введите фамилию плательщика");
        }

        private async Task ResumeAfterSurname(IDialogContext context, IAwaitable<string> result)
        {
            surname = await result;
            PromptDialog.Text(context, ResumeAfterName, "введите имя плательщика");
        }

        private async Task ResumeAfterName(IDialogContext context, IAwaitable<string> result)
        {
            name = await result;
            var shtrafiCLient = new ShtrafBizClient();
            var resp = shtrafiCLient.CreateZkz(shtrafsWantToPay, sts, vu, surname, name);
            if (resp?.Err != -1)
            {
                var buttonPay = new CardAction
                {
                    //  Value = "test",
                    Value = resp.Urlpay,
                    Type = "openUrl",
                    Title = "Оплатить"
                };
                await context.PostWithButtonsAsync($"оплата {shtrafsWantToPay.Count} штрафов",
                    new List<CardAction>() {buttonPay});
                await context.PostAsync($"Нажимая кнопку «Оплатить» вы принимаете условия Соглашения об использовании сервиса: https://shtraf.biz/doc_sogl.pdf");
                // await context.PostAsync($"Для оплаты перейдите по ссылке: {resp.Urlpay}");

                //todo: abstract out of telegram id
                await Subscribe(context);

            }
            else
            {
                await context.PostAsync($"При оплате произошла ошибка. Мы уже в курсе. Приносим свои извинения. Повторите поиск.");
            }

           
            /*var button = new CardAction
            {
                //  Value = "test",
                Value = $"искать ещё",
                Type = "imBack",
                Title = "искать ещё"
            };
            await context.PostWithButtonsAsync("", new List<CardAction>() {button});*/
        }

        private async Task Subscribe(IDialogContext context)
        {
           // var userId = context.Activity.From.Id;

            user = _shtrafiUserService.GetUserByMessengerId(user.UserIdTelegramm);
            if (user == null)
            {
              //  var registeredUser = _shtrafiUserService.RegisterUserAfterFirstPay(userId, name, surname, sts, vu);

                var subscribedDocSet = _shtrafiUserService.RegisterDocumentSetToCheck(user, sts, vu, name);

                var buttonOk = new CardAction
                {
                    //  Value = "test",
                    Value = "оставить",
                    Type = "imBack",
                    Title = "оставить"
                };
                var buttonCancel = new CardAction
                {
                    //  Value = "test",
                    Value = "отменить",
                    Type = "imBack",
                    Title = "отменить подписку"
                };
                var buttonNew = new CardAction
                {
                    //  Value = "test",
                    Value = "новый",
                    Type = "imBack",
                    Title = "новый поиск"
                };
                await context.PostWithButtonsAsync(
                    "Кстати, мы подписали вас на уведомления о новых штрафах. Сообщение будет приходить только если новый штраф найден. Но вы можете отменить подписку, если хотите.", new List<CardAction>(){buttonOk, buttonCancel, buttonNew });

                context.Wait(ResumeAfterSubscription);
            }
            else
            {
                if (user.DocumentSetsTocheck.FirstOrDefault(check => check.Sts == sts) == null)
                {
                    PromptDialog.Confirm(context, ResumeAfterAskToSaveSubscribtion,
                        "Сохранить подписку на новые штрафы для этих документов? Мы оповестим вас только если появится новый штраф и не будем надоедать сообщениями.");
                }
                else
                {
                    await context.PostAsync(
                    "У вас уже есть подписка об уведомлении по этому набору документов. Если хотите отменить введите **отменить подписку**");
                context.Done(1);
                }
                
            }
        }

        private async Task ResumeAfterAskToSaveSubscribtion(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                //if its first set of docs
                if (user.DocumentSetsTocheck == null || user?.DocumentSetsTocheck.Count == 0) await SaveSubscriptionOfDocSet(context, "основной");
                else
                {
                    name = "";
                    PromptDialog.Text(context, ResumeAfterNameOfDocSet, "дайте имя этому набору документов , например \"машина Мамы\"");
                }
            }
            else context.Done(1);
        }

        private async Task ResumeAfterNameOfDocSet(IDialogContext context, IAwaitable<string> result)
        {
            name = await result;
            await SaveSubscriptionOfDocSet(context, name);
        }

        private async Task SaveSubscriptionOfDocSet(IDialogContext context, string nameOfSet)
        {
            if (_shtrafiUserService.RegisterDocumentSetToCheck(user, sts, vu, nameOfSet) != null)
            {
                string txtVu = (vu != "") ?  $", Водительское: {vu}" : "";
                await context.PostAsync($"Подписка на набор документов: Свидетельство: {sts} {txtVu} успешно сохранена под именем **{nameOfSet}**");
                context.Done(1);
            }
            else
            {
                await context.PostAsync("При создании подписки произошла ошибка. Попробуйте снова.");
                await ResumeAfterAskToSaveSubscribtion(context, new AwaitableFromItem<bool>(true));
            }
        }

        private async Task ResumeAfterSubscription(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var res = await result;
            if (res.Text == "отменить")
            {
                //todo: add cancelling of subscription
            }
            if (res.Text == "новый")
            {
                context.Done(1);
                //todo: add cancelling of subscription
            }
        }
    }
}