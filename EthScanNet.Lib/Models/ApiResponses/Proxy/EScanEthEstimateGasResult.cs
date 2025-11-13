using System.Numerics;
using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthEstimateGasResult : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the estimated gas as a hex string
        /// </summary>
        public string GetEstimatedGasHex()
        {
            return Result?.ToString();
        }

        /// <summary>
        /// Get the estimated gas as a BigInteger
        /// </summary>
        public BigInteger? GetEstimatedGas()
        {
            var hex = GetEstimatedGasHex();
            if (string.IsNullOrEmpty(hex)) return null;
            
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            
            if (string.IsNullOrEmpty(hex)) return BigInteger.Zero;
            
            return BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
