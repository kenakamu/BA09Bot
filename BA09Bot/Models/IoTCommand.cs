using Newtonsoft.Json;

namespace BA09Bot.Models
{
    public class IoTCommand
    {
        [JsonProperty("operation")]
        public string Operation { get; set; }
        [JsonProperty("iotalert")]
        public IoTAlert Alert { get; set; }
        [JsonProperty("iotdevice")]
        public IoTDevice Device { get; set; }
        public class IoTAlert
        {
            [JsonProperty("msdyn_iotalertid")]
            public string IoTAlertId { get; set; }
            [JsonProperty("@odata.type")]
            public string Type = "Microsoft.Dynamics.CRM.msdyn_iotalert";
        }
        public class IoTDevice
        {
            [JsonProperty("msdyn_iotdeviceid")]
            public string IoTDeviceId { get; set; }
            [JsonProperty("@odata.type")]
            public string Type = "Microsoft.Dynamics.CRM.msdyn_iotdevice";
        }
    }
}