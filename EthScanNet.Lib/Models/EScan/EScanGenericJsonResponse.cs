using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.EScan
{
    internal class EScanGenericJsonResponse
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; }
        
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("result")]
        public object Result { get; set; }
    }
}