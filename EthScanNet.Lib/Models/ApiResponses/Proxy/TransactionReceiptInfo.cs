using EthScanNet.Lib.Models.ApiResponses.Logs;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class TransactionReceiptInfo
    {
        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty("blockNumber")]
        public string BlockNumber { get; set; }

        [JsonProperty("contractAddress")]
        public string ContractAddress { get; set; }

        [JsonProperty("cumulativeGasUsed")]
        public string CumulativeGasUsed { get; set; }

        [JsonProperty("effectiveGasPrice")]
        public string EffectiveGasPrice { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("gasUsed")]
        public string GasUsed { get; set; }

        [JsonProperty("logs")]
        public List<LogInfo> Logs { get; set; }

        [JsonProperty("logsBloom")]
        public string LogsBloom { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("transactionHash")]
        public string TransactionHash { get; set; }

        [JsonProperty("transactionIndex")]
        public string TransactionIndex { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class LogInfo
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("topics")]
        public List<string> Topics { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("blockNumber")]
        public string BlockNumber { get; set; }

        [JsonProperty("transactionHash")]
        public string TransactionHash { get; set; }

        [JsonProperty("transactionIndex")]
        public string TransactionIndex { get; set; }

        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty("logIndex")]
        public string LogIndex { get; set; }

        [JsonProperty("removed")]
        public bool Removed { get; set; }
    }
}
