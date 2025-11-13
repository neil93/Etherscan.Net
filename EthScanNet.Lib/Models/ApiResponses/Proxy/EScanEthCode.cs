using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthCode : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the contract bytecode as a hex string
        /// </summary>
        public string GetCode()
        {
            return Result?.ToString();
        }

        /// <summary>
        /// Check if the address has contract code (is a contract)
        /// </summary>
        public bool IsContract()
        {
            var code = GetCode();
            return !string.IsNullOrEmpty(code) && code != "0x";
        }
    }
}
