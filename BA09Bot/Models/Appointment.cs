using Newtonsoft.Json;
using System;

namespace BA09Bot.Models
{
    [Serializable]
    public class Appointment
    {
        [JsonProperty("activityid")]
        public string Id { get; set; }
        [JsonProperty("subject")]
        public string Title { get; set; }
        [JsonProperty("scheduledstart@OData.Community.Display.V1.FormattedValue")]
        public DateTime FormattedStartDate { get; set; }
        [JsonProperty("scheduledstart")]
        public DateTime StartDate { get; set; }
        [JsonProperty("scheduledend@OData.Community.Display.V1.FormattedValue")]
        public DateTime FormattedEndDate { get; set; }
        [JsonProperty("scheduledend")]
        public DateTime EndDate { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }

        // Used by PromptDialog to display this object
        public override string ToString()
        {
            return Title;
        }
    }
}