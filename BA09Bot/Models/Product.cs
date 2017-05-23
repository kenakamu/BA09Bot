using Newtonsoft.Json;
using System;

namespace BA09Bot.Models
{
    [Serializable]
    public class Product
    {
        [JsonProperty("productid")]
        public string Id { get; set; }
        [JsonProperty("productnumber")]
        public string Number { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("productstructure")]
        public string ProductStructure { get; set; }
        [JsonProperty("entityimage")]
        public string Image { get; set; }

        // Used by PromptDialog to display this object
        public override string ToString()
        {
            return Name;
        }
    }
}