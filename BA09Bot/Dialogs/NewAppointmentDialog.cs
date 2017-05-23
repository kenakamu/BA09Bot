using BA09Bot.Models;
using BA09Bot.Resources;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace BA09Bot.Dialogs
{
    [Serializable]
    public class NewAppointmentDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            // FormFlow is good way to create form, but enter datetime is troublesome....
            //var appointmentFormDialog = FormDialog.FromForm(this.BuildAppoointmentForm, FormOptions.None);
            //context.Call(appointmentFormDialog, this.ResumeAftersAppointmentFormDialog);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // Create Button with open browser
            var message = await result as Activity;
            var reply = message.CreateReply();
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = "list";

            List<CardAction> cardButtons = new List<CardAction>();

            CardAction plButton = new CardAction()
            {
                // LINE support http/https/tel protocol only
                //Value = $"ms-dynamicsxrm://?pagetype=create&etn=appointment",
                Value = $"{ConfigurationManager.AppSettings["D365Url"]}/_forms/mobilerefresh/page.aspx?etc=4201",
                Type = "openUrl",
                Title = Resource.Create
            };
            cardButtons.Add(plButton);
            ThumbnailCard plCard = new ThumbnailCard()
            {
                Title = Resource.Open_In_Browser,
                Images = null,
                Buttons = cardButtons,
                Text =Resource.Create_Appointment
            };
            reply.Attachments.Add(plCard.ToAttachment());

            await context.PostAsync(reply);
            context.Done(true);
        }

        /// <summary>
        /// Craete Appointment Form. 
        /// </summary>
        /// <returns></returns>
        private IForm<Appointment> BuildAppoointmentForm()
        {
            OnCompletionAsyncDelegate<Appointment> processAppointmentCreate = async (context, state) =>
            {
                await context.PostAsync(Resource.Creating_Appointment);
            };

            return new FormBuilder<Appointment>()
                .Message(Resource.Create_Appointment)
                .Field(nameof(Appointment.Title), new PromptAttribute(Resource.Ask_Subject))
                .Field(nameof(Appointment.Description), new PromptAttribute(Resource.Ask_Description))
                .Field(nameof(Appointment.StartDate), new PromptAttribute(Resource.Ask_StartDate))
                .Field(nameof(Appointment.EndDate), new PromptAttribute(Resource.Ask_EndDate))
                .OnCompletion(processAppointmentCreate)
                .Build();
        }       

        private async Task ResumeAftersAppointmentFormDialog(IDialogContext context, IAwaitable<Appointment> result)
        {
            context.Done<object>(null);
        }
    }
}