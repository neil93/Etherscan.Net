using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthTransaction : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the transaction as a strongly-typed TransactionInfo object
        /// </summary>
        public TransactionInfo GetTransactionInfo()
        {
            if (Result == null) return null;
            
            var json = JsonConvert.SerializeObject(Result);
            return JsonConvert.DeserializeObject<TransactionInfo>(json);
        }
    }
}
