using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using BA09Bot.Services;
using AuthBot;
using System.Configuration;
using System.Globalization;
using System.Collections.Generic;
using BA09Bot.Models;
using System.Web;
using BA09Bot.Resources;
using Autofac;

namespace BA09Bot.Dialogs
{
    [Serializable]
    public class SystemAlertDialog : IDialog<object>
    {
        private string title;
        private string description;
        private string deviceId;
        private string alertId;

        public SystemAlertDialog(string title, string description, string deviceId, string alertId)
        {
            this.title = title;
            this.description = description;
            this.deviceId = deviceId;
            this.alertId = alertId;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = (context.Activity as Activity).CreateReply();

            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = "carousel";

            List<CardImage> cardImages = new List<CardImage>();
            using (var scope = WebApiApplication.Container.BeginLifetimeScope())
            {
                IImageService imageService = scope.Resolve<IImageService>();

                CardImage cardImage = new CardImage(imageService.GetImageUri("alert.png"));
                cardImages.Add(cardImage);
                List<CardAction> cardButtons = new List<CardAction>();
                CardAction actionRestart = new CardAction()
                {
                    Value = Resource.Reboot,
                    Type = "postBack",
                    Title = Resource.Reboot
                };
                cardButtons.Add(actionRestart);

                CardAction actionShutDown = new CardAction()
                {
                    Value = Resource.Reset_To_Default,
                    Type = "postBack",
                    Title = Resource.Reset_To_Default
                };
                cardButtons.Add(actionShutDown);
                HeroCard plCard = new HeroCard()
                {
                    Title = title,
                    Subtitle = description,
                    Images = cardImages,
                    Buttons = cardButtons
                };
                reply.Attachments.Add(plCard.ToAttachment());
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            // If it is directline, then ignore the first one as it just said "check result"
            // But it check the result for second time.
            if (message.ChannelId.ToLower() == "directline" && message.Text == Resource.Confirm)
            {
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                var command = "";
                using (var scope = WebApiApplication.Container.BeginLifetimeScope())
                {
                    ID365Service d356Service = scope.Resolve<ID365Service>(new TypedParameter(typeof(IDialogContext), context));

                    if (message.Text == Resource.Reboot)
                    {
                        command = "reboot";
                    }
                    else if (message.Text == Resource.Reset_To_Default)
                    {
                        command = "reset";
                    }
                    await d356Service.CreateIoTCommand(command, alertId, deviceId);
                    await context.PostAsync(string.Format(Resource.Done_Operation, message.Text));
                    context.Done(true);
                }
            }
        }        
    }
}