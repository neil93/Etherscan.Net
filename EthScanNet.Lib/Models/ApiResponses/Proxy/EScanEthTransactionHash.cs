using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthTransactionHash : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the transaction hash
        /// </summary>
        public string GetTransactionHash()
        {
            return Result?.ToString();
        }
    }
}
