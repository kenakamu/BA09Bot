using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace BA09Bot.Services
{
    /// <summary>
    /// See https://github.com/Microsoft/BotBuilder-Samples/tree/master/CSharp/core-proactiveMessages for more detail.
    /// </summary>
    public class ConversationStarter
    {       
        /// <summary>
        /// Insert specified Dialog on top of current Dialog Stack.
        /// </summary>
        public static async Task Resume(IDialog<object> dialog, string userId, string channelId, string serviceUri)
        {
            var conversationReference = await GetConversationReference(userId, channelId, serviceUri);

            if (conversationReference == null)
                return;

            var storedMessage = conversationReference.GetPostToBotMessage();
            var client = new ConnectorClient(new Uri(storedMessage.ServiceUrl));

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, storedMessage))
            {
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync(CancellationToken.None);

                //This is our dialog stack
                var task = scope.Resolve<IDialogTask>();

                //interrupt the stack. This means that we're stopping whatever conversation that is currently happening with the user
                //Then adding this stack to run and once it's finished, we will be back to the original conversation
                task.Call(dialog.Void<object, IMessageActivity>(), null);

                await task.PollAsync(CancellationToken.None);

                //flush dialog stack
                await botData.FlushAsync(CancellationToken.None);
            }
        }
              
        /// <summary>
        /// Send message to current ongoing conversation
        /// </summary>
        public static async Task Resume(string message, string userId, string channelId, string serviceUri)
        {
            var connector = new ConnectorClient(new Uri(serviceUri));
            var conversationReference = await GetConversationReference(userId, channelId, serviceUri);

            IMessageActivity notification;

            // If not resumption data, then create new conversation.
            if (conversationReference == null)
            {
                var userAccount = new ChannelAccount(userId, "");
                var botAccount = new ChannelAccount(ConfigurationManager.AppSettings["BotId"], "");

                notification = Activity.CreateMessageActivity();
                var conversationId = (await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount)).Id;

                notification.From = botAccount;
                notification.Recipient = userAccount;
                notification.Conversation = new ConversationAccount(id: conversationId);
            }
            else
            {
                var storedMessage = conversationReference.GetPostToBotMessage();
                notification = storedMessage.CreateReply();
            }

            notification.Text = message;
            await connector.Conversations.SendToConversationAsync((Activity)notification);
        }

        public static async Task Reset(string userId, string channelId, string serviceUri)
        {
            var conversationReference = await GetConversationReference(userId, channelId, serviceUri);

            if (conversationReference == null)
                return;

            var storedMessage = conversationReference.GetPostToBotMessage();
            var client = new ConnectorClient(new Uri(storedMessage.ServiceUrl));

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, storedMessage))
            {
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync(CancellationToken.None);

                //This is our dialog stack
                var task = scope.Resolve<IDialogTask>();
                task.Reset();

                //flush dialog stack
                await botData.FlushAsync(CancellationToken.None);
            }
        }
        
        /// <summary>
        /// Get ResumptionCookie from state service
        /// </summary>
        private static async Task<ConversationReference> GetConversationReference(string userId, string channelId, string serviceUri)
        {
            if (channelId != "emulator")
                serviceUri = "https://state.botframework.com";

            StateClient stateClient = new StateClient(new Uri(serviceUri), new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]));
            BotData userState = null;
            try
            {
                userState = await stateClient.BotState.GetUserDataAsync(channelId, userId);
            }
            catch (Exception ex)
            {
                // If no userState, then do nothing
                return null;
            }
            return JsonConvert.DeserializeObject<ConversationReference>(userState.GetProperty<string>("ConversationReference"));
        }


        /*
        /// <summary>
        /// Insert specified Dialog on top of current Dialog Stack.
        /// </summary>
        public static async Task Resume(IDialog<object> dialog, string userId, string channelId, string serviceUri)
        {
            var resumptionCookie = await GetResumptionCookie(userId, channelId, serviceUri);
            var ConversationReference = await GetConversationReference(userId, channelId, serviceUri);

            if (string.IsNullOrEmpty(resumptionCookie))
                return;

            var storedMessage = ResumptionCookie.GZipDeserialize(resumptionCookie).GetMessage();
            var client = new ConnectorClient(new Uri(storedMessage.ServiceUrl));

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, storedMessage))
            {
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync(CancellationToken.None);

                //This is our dialog stack
                var task = scope.Resolve<IDialogTask>();

                //interrupt the stack. This means that we're stopping whatever conversation that is currently happening with the user
                //Then adding this stack to run and once it's finished, we will be back to the original conversation
                task.Call(dialog.Void<object, IMessageActivity>(), null);

                await task.PollAsync(CancellationToken.None);

                //flush dialog stack
                await botData.FlushAsync(CancellationToken.None);
            }
        }

        /// <summary>
        /// Send message to current ongoing conversation
        /// </summary>
        public static async Task Resume(string message, string userId, string channelId, string serviceUri)
        {
            var connector = new ConnectorClient(new Uri(serviceUri));
            var resumptionCookie = await GetResumptionCookie(userId, channelId, serviceUri);

            IMessageActivity notification;

            // If not resumption data, then create new conversation.
            if (string.IsNullOrEmpty(resumptionCookie))
            {
                var userAccount = new ChannelAccount(userId, "");
                var botAccount = new ChannelAccount(ConfigurationManager.AppSettings["BotId"], "");

                notification = Activity.CreateMessageActivity();
                var conversationId = (await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount)).Id;

                notification.From = botAccount;
                notification.Recipient = userAccount;
                notification.Conversation = new ConversationAccount(id: conversationId);
            }
            else
            {
                var storedMessage = ResumptionCookie.GZipDeserialize(resumptionCookie).GetMessage();
                notification = storedMessage.CreateReply();
            }

            notification.Text = message;
            await connector.Conversations.SendToConversationAsync((Activity)notification);
        }

        public static async Task Reset(string userId, string channelId, string serviceUri)
        {
            var resumptionCookie = await GetResumptionCookie(userId, channelId, serviceUri);

            if (string.IsNullOrEmpty(resumptionCookie))
                return;

            var storedMessage = ResumptionCookie.GZipDeserialize(resumptionCookie).GetMessage();
            var client = new ConnectorClient(new Uri(storedMessage.ServiceUrl));

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, storedMessage))
            {
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync(CancellationToken.None);

                //This is our dialog stack
                var task = scope.Resolve<IDialogTask>();
                task.Reset();


                //flush dialog stack
                await botData.FlushAsync(CancellationToken.None);
            }
        }

        /// <summary>
        /// Get ResumptionCookie from state service
        /// </summary>
        private static async Task<string> GetResumptionCookie(string userId, string channelId, string serviceUri)
        {
            switch (channelId)
            {
                case "directline":
                    serviceUri = "https://state.botframework.com";
                    break;
            }

            StateClient stateClient = new StateClient(new Uri(serviceUri), new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]));
            BotData userState = null;
            try
            {
                userState = await stateClient.BotState.GetUserDataAsync(channelId, userId);
            }
            catch (Exception ex)
            {
                // If no userState, then do nothing
                return "";
            }
            return userState.GetProperty<string>("ResumptionCookie");
        }
        */
    }
}