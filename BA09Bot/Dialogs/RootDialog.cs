using AuthBot;
using AuthBot.Dialogs;
using Autofac;
using BA09Bot.Resources;
using BA09Bot.Services;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace BA09Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;
            
            // Store ConversationReference for Resume scenario.
            context.UserData.SetValue<string>("ConversationReference", JsonConvert.SerializeObject(activity.ToConversationReference()));

            // Check if User is authenticated
            if (string.IsNullOrEmpty(await context.GetAccessToken(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"])))
            {
                await context.Forward(new AzureAuthDialog(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"], Resource.SignIn_Prompt), this.ResumeAfterAuth, context.Activity, CancellationToken.None);
            }
            else
            {
                // Insert LUIS here if you want to do natural language understanding and use TopIntent to dispatch sub dialog.

                if (activity.Text == Resource.Menu_SearchProduct)
                {
                    // Call child dialog without passing user input.
                    context.Call(new SearchProductDialog(), ResumeAfterDialogs);
                }
                else if (activity.Text == Resource.Menu_Appointment_For_Today)
                {
                    // Call child dialog by passing user input.
                    await context.Forward(new AppointmentForTodayDialog(), ResumeAfterDialogs, activity, CancellationToken.None);
                }
                else if (activity.Text == Resource.Menu_Create_Appointment)
                {
                    await context.Forward(new NewAppointmentDialog(), ResumeAfterDialogs, activity, CancellationToken.None);
                }
                else if (activity.Text == Resource.Menu_Logout)
                {
                    await context.Logout();
                }
                else if (activity.Text == Resource.Menu_Task_Menu)
                {
                    context.Call(new TaskDialog(), ResumeAfterDialogs);
                }
                else if (activity.Text == Resource.Menu_Daily_Report)
                {
                    context.Call(new DailyReportDialog(), ResumeAfterDialogs);
                }
                else
                {
                    await ShowHelp(context);
                }
            }
        }
     
        private async Task ResumeAfterDialogs(IDialogContext context, IAwaitable<object> result)
        {
            await ShowHelp(context);
        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            await context.PostAsync(message);
            using (var scope = WebApiApplication.Container.BeginLifetimeScope())
            {
                ID365Service d356Service = scope.Resolve<ID365Service>(new TypedParameter(typeof(IDialogContext), context));
                
                await d356Service.UpdateUserInfoForBot();
            }
            await ShowHelp(context);
        }

        private async Task ShowHelp(IDialogContext context)
        {
            if (context.Activity.ChannelId == "directline")
            {
                var menu = $@"-{Resource.Menu_SearchProduct}
-{Resource.Menu_Appointment_For_Today}
-{Resource.Menu_Create_Appointment}
-{Resource.Menu_Daily_Report}
-{Resource.Menu_Task_Menu}
-{Resource.Menu_Logout}
-{Resource.Menu_Help}";
                await context.PostAsync(string.Format(Resource.Help, menu));
            }
            else
                PromptDialog.Choice(context, MenuSelected, new List<string>()
                {
                    Resource.Menu_SearchProduct,
                    Resource.Menu_Appointment_For_Today,
                    Resource.Menu_Create_Appointment,
                    Resource.Menu_Daily_Report,
                    Resource.Menu_Task_Menu,
                    Resource.Menu_Logout,
                    Resource.Menu_Help }, Resource.Menu_Title);
        }

        private async Task MenuSelected(IDialogContext context, IAwaitable<string> result)
        {
            // This never called as we reset Dialog Stack of preset menus at MessageController
        }
    }
}