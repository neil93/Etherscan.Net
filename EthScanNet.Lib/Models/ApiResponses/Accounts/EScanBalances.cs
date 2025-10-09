using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Accounts
{
    public class EScanAccountBalance
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("balance")]
        public BigInteger Balance { get; set; }
    }

    public class EScanBalances : EScanResponse
    {
        [JsonProperty("result")]
        public List<EScanAccountBalance> Balances { get; set; }
    }
}