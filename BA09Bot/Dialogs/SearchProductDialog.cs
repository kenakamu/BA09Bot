using Autofac;
using BA09Bot.Resources;
using BA09Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace BA09Bot.Dialogs
{
    [Serializable]
    public class SearchProductDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(Resource.Enter_ProductName);
            context.Wait(SearchProductAsync);
        }
     
        private async Task SearchProductAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result as Activity;

            using (var scope = WebApiApplication.Container.BeginLifetimeScope())
            {
                ID365Service d356Service = scope.Resolve<ID365Service>(new TypedParameter(typeof(IDialogContext), context));

                var products = await d356Service.GetProducts(message.Text);
                if (products.Count > 5)
                {
                    await context.PostAsync(string.Format(Resource.Retry_More_Than_x_Items, 5));
                    context.Wait(SearchProductAsync);
                }
                else
                {

                    Activity reply = message.CreateReply();
                    reply.AttachmentLayout = "carousel";
                    reply.Attachments = new List<Attachment>();

                    foreach (var product in products)
                    {
                        IAzureBlobService blobService = scope.Resolve<IAzureBlobService>();

                        var imageUrl = blobService.Upload(product.Image, product.Number);
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: imageUrl));

                        List<CardAction> cardButtons = new List<CardAction>();

                        CardAction plButton = new CardAction()
                        {
                            //Value = $"ms-dynamicsxrm://?pagetype=create&etn=appointment",
                            Value = $"{ConfigurationManager.AppSettings["D365Url"]}/m/ef.aspx?etn=product&id=%7b{product.Id}%7d",
                            Type = "openUrl",
                            Title = Resource.Detail
                        };

                        cardButtons.Add(plButton);

                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = product.Name,
                            Subtitle = product.Description,
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply.Attachments.Add(plAttachment);
                    }

                    await context.PostAsync(reply);
                    context.Done(true);
                }
            }
        }
    }
}