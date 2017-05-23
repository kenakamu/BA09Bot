using Autofac;
using BA09Bot.Resources;
using BA09Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace BA09Bot.Dialogs
{
    [Serializable]
    public class DailyReportDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(Resource.Ask_DailyReport);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.ChannelId.ToLower() == "directline" && message.Text == Resource.Confirm)
            {
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                using (var scope = WebApiApplication.Container.BeginLifetimeScope())
                {
                    ID365Service d356Service = scope.Resolve<ID365Service>(new TypedParameter(typeof(IDialogContext), context));

                    await d356Service.CreateDailyReport(message.Text);
                    await context.PostAsync(Resource.Created_DailyReport);
                    context.Done(true);
                }
            }
        }        
    }
}