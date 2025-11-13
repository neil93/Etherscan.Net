using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthTransactionReceipt : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the transaction receipt as a strongly-typed TransactionReceiptInfo object
        /// </summary>
        public TransactionReceiptInfo GetReceiptInfo()
        {
            if (Result == null) return null;
            
            var json = JsonConvert.SerializeObject(Result);
            return JsonConvert.DeserializeObject<TransactionReceiptInfo>(json);
        }
    }
}
