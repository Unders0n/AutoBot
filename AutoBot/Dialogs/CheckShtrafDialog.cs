﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StepApp.BotExtensions.DialogExtensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Model.Entities;
using ShrafiBiz.Client;
using ShrafiBiz.Model;
using ShtrafiBLL;

namespace AutoBot.Dialogs
{
    [Serializable]
    public class CheckShtrafDialog : IDialog<object>
    {
        private const int MAX_RETRIES = 3;

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

        private async Task TryCheckPay(IDialogContext context, ShtrafBizClient shtrafiCLient)
        {
            pays = shtrafiCLient.CheckPay(sts, vu);

            //нет штрафов
            if (pays.Err == -4)
            {
                await context.PostAsync("Начисления не найдены.");
                CheckUserRegisterAndSubscribe(context);

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
            else if (pays.Err == 0)
            {
                shtrafsAll = pays.L;
                await PrintPaymentDetails(context);
            }


           
        }

        private async Task PrintPaymentDetails(IDialogContext context)
        {
            var i = 1;
            foreach (var pay in pays.L)
                await context.PostAsync($"{i++}: {pay.Value}");

            var totalSumm = pays.L.Sum(pair => Int16.Parse(pair.Value.Sum));
            var totalSummFeesrv = pays.L.Sum(pair => pair.Value.Feesrv);
            await context.PostAsync(
                $"Всего **{pays.L.Count}** штрафов на общую сумму **{totalSumm}**р (+ {totalSummFeesrv}р комиссия)");


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


            var cardForButton = new ThumbnailCard {Buttons = new List<CardAction> {buttonPay}};
            mes.Attachments.Add(cardForButton.ToAttachment());

            await context.PostAsync(mes);
            context.Wait(AfterSelectShtrafsToPay);
        }

        private async Task ResumeAfterVuEntered(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var txt = await result;
            if (txt.Text == "пропустить")
                vu = "";
            else
                vu = txt.Text;

            var shtrafiCLient = new ShtrafBizClient();

            await TryCheckPay(context, shtrafiCLient);
        }

        private async Task AfterSelectShtrafsToPay(IDialogContext context, IAwaitable<object> result)
        {
            var resu = await result;
            var res = resu as Activity;

            var txt = res.Text;

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
            PromptDialog.Text(context, ResumeAfterShtrafs, "введите фамилию плательщика");
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
                CheckUserRegisterAndSubscribe(context);

            }
            else
            {
                await context.PostAsync($"При оплате произошла ошибка. Мы уже в курсе. Приносим свои извинения. Повторите поиск.");
            }

            context.Done(1);
            /*var button = new CardAction
            {
                //  Value = "test",
                Value = $"искать ещё",
                Type = "imBack",
                Title = "искать ещё"
            };
            await context.PostWithButtonsAsync("", new List<CardAction>() {button});*/
        }

        private async void CheckUserRegisterAndSubscribe(IDialogContext context)
        {
            var userId = context.Activity.From.Id;

            user = _shtrafiUserService.GetUserByMessengerId(context.Activity.From.Name);
            if (user == null)
            {
                var registeredUser = _shtrafiUserService.RegisterUserAfterFirstPay(userId, name, surname, sts, vu);

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
                        "Сохранить подписку на новые штрафы для этого набора документов?");
                }
                
            }
        }

        private async Task ResumeAfterAskToSaveSubscribtion(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                string name = "";
                PromptDialog.Text(context, async (dialogContext, awaitable) => name = await awaitable, "дайте имя этому набору документов , например \"машина Мамы\"");
                if (_shtrafiUserService.RegisterDocumentSetToCheck(user, sts, vu, name) != null)
                {
                    
                }
                else
                {
                    await context.PostAsync("При создании подписки произошла ошибка. Попробуйте снова.");
                    await ResumeAfterAskToSaveSubscribtion(context, new AwaitableFromItem<bool>(true));
                }
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