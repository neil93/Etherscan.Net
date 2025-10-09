using System.Collections.Generic;
using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Logs
{
    public class EScanLog
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("topics")]
        public List<string> Topics { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("blockNumber")]
        public string BlockNumber { get; set; }

        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }
        
        [JsonProperty("timeStamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("gasPrice")]
        public string GasPrice { get; set; }

        [JsonProperty("gasUsed")]
        public string GasUsed { get; set; }

        [JsonProperty("logIndex")]
        public string LogIndex { get; set; }

        [JsonProperty("transactionHash")]
        public string TransactionHash { get; set; }

        [JsonProperty("transactionIndex")]
        public string TransactionIndex { get; set; }
    }

    public class EScanLogs : EScanResponse
    {
        [JsonProperty("result")]
        public List<EScanLog> Logs { get; set; }
    }
}