using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthCallResult : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the call result as a hex string
        /// </summary>
        public string GetResultHex()
        {
            return Result?.ToString();
        }
    }
}
