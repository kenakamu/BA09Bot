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
    public class ApproveDialog : IDialog<object>
    {
        private string title;
        private string description;

        public ApproveDialog(string title, string description)
        {
            this.title = title;
            this.description = description;
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
                CardAction actionApprove = new CardAction()
                {
                    Value = Resource.Approve,
                    Type = "postBack",
                    Title = Resource.Approve
                };
                cardButtons.Add(actionApprove);

                CardAction actionSuspend = new CardAction()
                {
                    Value = Resource.Suspend,
                    Type = "postBack",
                    Title = Resource.Suspend
                };
                cardButtons.Add(actionSuspend);

                CardAction actionReject = new CardAction()
                {
                    Value = Resource.Reject,
                    Type = "postBack",
                    Title = Resource.Reject
                };
                cardButtons.Add(actionReject);
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
                // Do approve operation here
                await context.PostAsync(string.Format(Resource.Done_Operation, message.Text));
                context.Done(true);
            }
        }        
    }
}