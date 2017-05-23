using Autofac;
using BA09Bot.Models;
using BA09Bot.Resources;
using BA09Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA09Bot.Dialogs
{
    [Serializable]
    public class TaskDialog : IDialog<object>
    {
        private List<ITask> tasks = new List<ITask>();
        private int currentTask = 0;

        public async Task StartAsync(IDialogContext context)
        {
            PromptChoice(context);
        }

        private async Task TaskMenuSelected(IDialogContext context, IAwaitable<string> result)
        {
            var selection = await result;
            if(selection == Resource.Get_Tasks)
            {
                await GetTasks(context);
                if (tasks.Count() == 0)
                {
                    await context.PostAsync(Resource.No_Tasks);
                    context.Done(true);
                }
                else
                {
                    string replyMessage = "";
                    foreach (var task in tasks.OrderBy(x => x.DueDateTime))
                    {
                        var dueDate = task.DueDateTime.ToString("MM/dd HH:mm", Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        replyMessage += $"{dueDate} {task.Title}{Environment.NewLine}";
                    }
                    var reply = (context.Activity as Activity).CreateReply(replyMessage);
                    await context.PostAsync(reply);
                }
            }
            else if(selection == Resource.Get_NextTask)
            {
                if(tasks.Count == 0)
                    await GetTasks(context);
                if (tasks.Count == currentTask)
                    await context.PostAsync(Resource.No_Tasks);
                else
                {
                    var task = tasks[currentTask];
                    var dueDate = task.DueDateTime.ToString("MM/dd HH:mm", Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                    await context.PostAsync($"{dueDate} {task.Title}-{task.Description}");
                    currentTask++;
                }
            }
            else if(selection == Resource.Finish)
            {
                context.Done(true);
                return;
            }

            PromptChoice(context);
        }
        
        private void PromptChoice(IDialogContext context)
        {
            PromptDialog.Choice(context, TaskMenuSelected, new List<string>() { Resource.Get_Tasks, Resource.Get_NextTask, Resource.Finish }, Resource.Select_Menu);
        }

        private async Task GetTasks(IDialogContext context)
        {
            tasks.Clear();
            using (var scope = WebApiApplication.Container.BeginLifetimeScope())
            {
                ID365Service d356Service = scope.Resolve<ID365Service>(new TypedParameter(typeof(IDialogContext), context));
                IO365Service o365Service = scope.Resolve<IO365Service>(new TypedParameter(typeof(IDialogContext), context));

                var crmTasks = await d356Service.GetTasks();
                var plannerTasks = await o365Service.GetTasks();
                tasks.AddRange(crmTasks);
                tasks.AddRange(plannerTasks);
                tasks = tasks.OrderBy(x => x.DueDateTime).ToList();
            }
        }
    }
}