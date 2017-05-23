using BA09Bot.Models;
using BA09Bot.Resources;
using BA09Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace BA09Bot.Dialogs
{
    [Serializable]
    public class AppointmentForTodayDialog : IDialog<object>
    {
        private List<Appointment> appointments;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result as Activity;
            using (var scope = WebApiApplication.Container.BeginLifetimeScope())
            {
                ID365Service d356Service = scope.Resolve<ID365Service>(new TypedParameter(typeof(IDialogContext), context));

                appointments = await d356Service.GetAppointmentsForToday();

                if (appointments.Count() == 0)
                {
                    await context.PostAsync(Resource.No_Appointment_Today);
                    context.Done(true);
                }
                else
                {
                    foreach (var appointment in appointments.OrderBy(x => x.FormattedStartDate))
                    {
                        var startTime = appointment.FormattedStartDate.ToString("t", Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        var endTime = appointment.FormattedEndDate.ToString("t", Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        var reply = message.CreateReply($"{startTime}-{endTime}:{appointment.Title}");
                        await context.PostAsync(reply);
                    }
                    appointments.Add(new Appointment() { Title = Resource.Finish });
                    DisplayChoice(context);
                }
            }
        }

        private void DisplayChoice(IDialogContext context)
        {
            PromptDialog.Choice<Appointment>(context, DisplayAppointmentDetail, appointments, Resource.Select_For_Detail, null, 0);
        }

        private async Task DisplayAppointmentDetail(IDialogContext context, IAwaitable<Appointment> result)
        {
            var appointment = await result;
            if (appointment.Title == Resource.Finish)
            {
                context.Done(true);
            }
            else
            {
                await context.PostAsync($"{appointment.Description}");
                DisplayChoice(context);
            }
        }
    }
}