using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class Withdrawal
    {
        [JsonProperty("index")]
        public string Index { get; set; }

        [JsonProperty("validatorIndex")]
        public string ValidatorIndex { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
