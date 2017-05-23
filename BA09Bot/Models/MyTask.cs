using Newtonsoft.Json;
using System;

namespace BA09Bot.Models
{
    [Serializable]
    public class CrmTask : ITask
    {
        [JsonProperty("activityid")]
        public override string Id { get; set; }
        [JsonProperty("subject")]
        public override string Title { get; set; }
        [JsonProperty("scheduledend@OData.Community.Display.V1.FormattedValue")]
        public override DateTime DueDateTime { get; set; }
        [JsonProperty("description")]
        public override string Description { get; set; }
        public CrmTask()
        {
            Type = "D365";
        }
    }

    [Serializable]
    public class PlannerTask : ITask
    {
        [JsonProperty("id")]
        public override string Id { get; set; }
        [JsonProperty("title")]
        public override string Title { get; set; }
        [JsonProperty("dueDateTime")]
        public override DateTime DueDateTime { get; set; }
        [JsonProperty("description")]
        public override string Description { get; set; }
        [JsonProperty("hasDescription")]
        public bool HasDescription { get; set; }

        public PlannerTask()
        {
            Type = "O365";
        }
    }

    [Serializable]
    public class ITask
    {
        public virtual string Id { get; set; }
        public virtual string Title { get; set; }
        public virtual DateTime DueDateTime { get; set; }
        public virtual string Description { get; set; }      
        public virtual string Type { get; set; }
    }
}