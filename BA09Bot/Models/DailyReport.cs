using Newtonsoft.Json;

namespace BA09Bot.Models
{
    public class DailyReport
    {
        [JsonProperty("comment")]
        public string Comment { get; set; }
        [JsonProperty("user")]
        public User RegardingUser { get; set; }
        public class User
        {
            [JsonProperty("systemuserid")]
            public string SystemUserId { get; set; }
            [JsonProperty("@odata.type")]
            public string Type { get; set; }
        }
    }
}