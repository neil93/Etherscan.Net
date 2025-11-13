using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class TransactionInfo
    {
        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty("blockNumber")]
        public string BlockNumber { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("gas")]
        public string Gas { get; set; }

        [JsonProperty("gasPrice")]
        public string GasPrice { get; set; }

        [JsonProperty("maxFeePerGas")]
        public string MaxFeePerGas { get; set; }

        [JsonProperty("maxPriorityFeePerGas")]
        public string MaxPriorityFeePerGas { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("input")]
        public string Input { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("transactionIndex")]
        public string TransactionIndex { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("accessList")]
        public object[] AccessList { get; set; }

        [JsonProperty("chainId")]
        public string ChainId { get; set; }

        [JsonProperty("v")]
        public string V { get; set; }

        [JsonProperty("r")]
        public string R { get; set; }

        [JsonProperty("s")]
        public string S { get; set; }

        [JsonProperty("yParity")]
        public string YParity { get; set; }
    }
}
