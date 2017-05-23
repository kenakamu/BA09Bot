using BA09Bot.Dialogs;
using BA09Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BA09Bot.Controllers
{
    /// <summary>
    /// Receive notification request from external system. You may want to secure the API.
    /// </summary>
    public class SendController : ApiController
    {
        [HttpPost]
        public async Task NotifySystemAlert([FromUri]string title, [FromUri]string description, [FromUri]string deviceId, [FromUri]string alertId, [FromUri]string userId, [FromUri]string channelId, [FromUri]string serviceUri, [FromUri]int lcid)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lcid);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lcid);

            var dialog = new SystemAlertDialog(title, description, deviceId, alertId);
            await Notify(dialog, userId, channelId, serviceUri, lcid);
        }

        [HttpPost]
        public async Task NotifyApprover([FromUri]string title, [FromUri]string description, [FromUri]string userId, [FromUri]string channelId, [FromUri]string serviceUri, [FromUri]int lcid)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lcid);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lcid);

            var dialog = new ApproveDialog(title, description);
            await Notify(dialog, userId, channelId, serviceUri ,lcid);
        }

        [HttpPost]
        public async Task NotifyDailyReport([FromUri]string userId, [FromUri]string channelId, [FromUri]string serviceUri, [FromUri]int lcid)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lcid);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lcid);

            var dialog = new DailyReportDialog();
            await Notify(dialog, userId, channelId, serviceUri, lcid);
        }
        
        [HttpPost]
        public async Task NotifyMessage([FromUri]string message, [FromUri]string userId, [FromUri]string channelId, [FromUri]string serviceUri, [FromUri]int lcid)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lcid);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lcid);

            await Notify(message, userId, channelId, serviceUri, lcid);
        }

        /// <summary>
        /// Notify the user with the Dialog
        /// </summary>
        private async Task Notify(IDialog<object> dialog, string userId, string channelId, string serviceUri, int lcid)
        {
            await ConversationStarter.Resume(dialog, userId, channelId, serviceUri);

            if (channelId.ToLower() == "directline")
            {
                var url = HttpContext.Current.Request.Url;
                var webUrl = $"{url.Scheme}://{url.Host}:{url.Port}/api/LineMessages/Notify?mids={userId}&lcid={lcid}";
                using (HttpClient client = new HttpClient())
                {
                    await client.PostAsync(webUrl, null);
                }
            }
        }

        /// <summary>
        /// Notify the user with message
        /// </summary>
        private async Task Notify(string message, string userId, string channelId, string serviceUri, int lcid)
        {            
            // As directline doesn't support push, directly call push for LINE channel
            if (channelId.ToLower() == "directline")
            {
                var url = HttpContext.Current.Request.Url;
                var webUrl = $"{url.Scheme}://{url.Host}:{url.Port}/api/LineMessages/Notify?message={message}&mids={userId}&lcid={lcid}";
                using (HttpClient client = new HttpClient())
                {
                    await client.PostAsync(webUrl, null);
                }
            }
            else
            {
                await ConversationStarter.Resume(message, userId, channelId, serviceUri);
            }
        }
    }
}