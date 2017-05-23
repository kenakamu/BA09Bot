using AuthBot;
using BA09Bot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BA09Bot.Services
{
    public class D365Service : ID365Service
    {
        IDialogContext context;

        public D365Service(IDialogContext context)
        {
            this.context = context;
        }
        
        /// <summary>
        /// Store Bot User information
        /// </summary>
        public async Task UpdateUserInfoForBot()
        {
            using (HttpClient client = await GetClient(true))
            {
                var activity = context.Activity;
                var result = await client.GetAsync($"api/data/v8.2/WhoAmI");
                if (result.IsSuccessStatusCode)
                {
                    var systemUserId = JToken.Parse(await result.Content.ReadAsStringAsync())["UserId"].ToString();
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"api/data/v8.2/systemusers({systemUserId})");
                    request.Content = new StringContent(
                        $"{{'bf_userid':'{activity.From.Id}','bf_channelid':'{activity.ChannelId}','bf_serviceurl':'{activity.ServiceUrl}'}}",
                        Encoding.UTF8, "application/json");
                    result = await client.SendAsync(request);
                }
                else
                {     
                    // Handle error
                }
            }
        }

        /// <summary>
        /// Create ActivityFeed Post by using Custom Action
        /// </summary>
        public async Task CreateDailyReport(string report)
        {
            using (HttpClient client = await GetClient(true))
            {
                var activity = context.Activity;
                var result = await client.GetAsync($"api/data/v8.2/WhoAmI");
                if (result.IsSuccessStatusCode)
                {
                    var systemUserId = JToken.Parse(await result.Content.ReadAsStringAsync())["UserId"].ToString();
                    DailyReport dailyReport = new DailyReport() { Comment = report, RegardingUser = new DailyReport.User() { SystemUserId = systemUserId, Type = "Microsoft.Dynamics.CRM.systemuser" } };
                    result = await client.PostAsync("api/data/v8.2/new_CreateDailyReport", new StringContent(JsonConvert.SerializeObject(dailyReport), Encoding.UTF8, "application/json"));
                }
                else
                {
                    // Handle error
                }
            }
        }

        /// <summary>
        /// Get Product Information by using FetchXml
        /// </summary>
        public async Task<List<Product>> GetProducts(string query)
        {
            using (HttpClient client = await GetClient(true))
            {
                string fetch = String.Format($@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='product'>
    <attribute name='name' />
    <attribute name='productid' />
    <attribute name='productnumber' />
    <attribute name='description' />
    <attribute name='statecode' />
    <attribute name='entityimage' />
    <attribute name='productstructure' />
    <order attribute='productnumber' descending='false' />
    <filter type='and'>
      <condition attribute='iskit' operator='eq' value='0' />
      <condition attribute='name' operator='like' value='%{query}%' />
    </filter>
  </entity>
</fetch>");
                var result = await client.GetAsync($"api/data/v8.2/products?fetchXml={Uri.EscapeUriString(fetch)}");
                if (result.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Product>>(JToken.Parse(await result.Content.ReadAsStringAsync())["value"].ToString());
                }
                else
                {
                    return new List<Product>();
                }
            }
        }

        /// <summary>
        /// Get Appointment Information by using FetchXml
        /// </summary>
        /// <returns></returns>
        public async Task<List<Appointment>> GetAppointmentsForToday()
        {
            using (HttpClient client = await GetClient(true))
            {
                string fetch = String.Format($@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='appointment'>
    <attribute name='subject' />
    <attribute name='scheduledstart' />
    <attribute name='scheduledend' />
    <attribute name='activityid' />
    <order attribute='scheduledstart' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='in'>
        <value>0</value>
        <value>3</value>
      </condition>
      <condition attribute='scheduledstart' operator='next-x-hours' value='24' />
      <condition attribute='scheduledstart' operator='today' />
    </filter>
    <link-entity name='activityparty' from='activityid' to='activityid' alias='ag'>
      <filter type='and'>
        <condition attribute='partyid' operator='eq-userid' />
        <condition attribute='participationtypemask' operator='in'>
          <value>7</value>
          <value>9</value>
          <value>5</value>
          <value>6</value>
        </condition>
      </filter>
    </link-entity>
  </entity>
</fetch>");
                var result = await client.GetAsync($"api/data/v8.2/appointments?fetchXml={Uri.EscapeUriString(fetch)}");
                if (result.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Appointment>>(JToken.Parse(await result.Content.ReadAsStringAsync())["value"].ToString());
                }
                else
                {
                    return new List<Appointment>();
                }
            }
        }
        
        /// <summary>
        /// Get Tasks
        /// </summary>
        /// <returns></returns>
        public async Task<List<CrmTask>> GetTasks()
        {
            using (HttpClient client = await GetClient(true))
            {
                var tasks = new List<CrmTask>();
                var result = await client.GetAsync($"api/data/v8.2/WhoAmI");
                if (result.IsSuccessStatusCode)
                {
                    var systemUserId = JToken.Parse(await result.Content.ReadAsStringAsync())["UserId"].ToString();
                    result = await client.GetAsync($"api/data/v8.2/tasks?$select=subject,prioritycode,description,scheduledend&$filter=statecode eq 0 and _ownerid_value eq {systemUserId}");
                    if (result.IsSuccessStatusCode)
                    {
                        tasks = JsonConvert.DeserializeObject<List<CrmTask>>(JToken.Parse(await result.Content.ReadAsStringAsync())["value"].ToString());
                    }
                }
                return tasks;
            }
        }
        
        /// <summary>
        /// Create IoT command for IoT Alert via Custom Action
        /// </summary>
        public async Task CreateIoTCommand(string operation, string alertId, string deviceId)
        {
            using (HttpClient client = await GetClient(true))
            {
                IoTCommand command = new IoTCommand()
                {
                 Operation = operation,
                  Alert = new IoTCommand.IoTAlert() { IoTAlertId = alertId },
                  Device = new IoTCommand.IoTDevice() { IoTDeviceId = deviceId}
                };

                var result = await client.PostAsync($"api/data/v8.2/new_CreateIoTCommand", new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json"));              
            }
        }

        private async Task<HttpClient> GetClient(bool isGet = false, int paging = 0)
        {
            HttpClient client = new HttpClient(new HttpClientHandler()
            { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["D365Url"]);

            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", 
                await context.GetAccessToken(ConfigurationManager.AppSettings["D365Url"]));

            if (isGet)
            {
                client.DefaultRequestHeaders.Add(
                    "Accept", "application/json");
                client.DefaultRequestHeaders.Add(
                    "Prefer", "odata.include-annotations=\"*\"");
            }
            if(paging != 0)
            {
                client.DefaultRequestHeaders.Add(
                    "Preference-Applied", $"odata.maxpagesize={paging}");
            }

            return client;
        }
    }      
}
