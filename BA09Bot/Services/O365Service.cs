using AuthBot;
using BA09Bot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BA09Bot.Services
{
    public class O365Service : IO365Service
    {
        IDialogContext context;

        public O365Service(IDialogContext context)
        {
            this.context = context;
        }
        
        /// <summary>
        /// Get Tasks from Planner
        /// </summary>
        /// <returns></returns>
        public async Task<List<PlannerTask>> GetTasks()
        {
            using (HttpClient client = await GetClient(true))
            {
                List<PlannerTask> tasks = new List<PlannerTask>();
                var result = await client.GetAsync($"beta/me/planner/tasks");
                if (result.IsSuccessStatusCode)
                {
                    tasks = JsonConvert.DeserializeObject<List<PlannerTask>>(JToken.Parse(await result.Content.ReadAsStringAsync())["value"].ToString());
                    foreach(var task in tasks.Where(x=>x.HasDescription == true))
                    {
                        result = await client.GetAsync($"beta/planner/tasks/{task.Id}/details");
                        if (result.IsSuccessStatusCode)
                        {
                            task.Description= JToken.Parse(await result.Content.ReadAsStringAsync())["description"].ToString();                          
                        }
                    }
                }

                return tasks;
            }
        }

        private async Task<HttpClient> GetClient(bool isGet = false)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["O365Url"]);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await context.GetAccessToken(ConfigurationManager.AppSettings["O365Url"]));
            
            return client;
        }
    }      
}
