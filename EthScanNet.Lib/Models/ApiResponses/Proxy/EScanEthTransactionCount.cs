using System.Numerics;
using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthTransactionCount : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the transaction count as a hex string
        /// </summary>
        public string GetCountHex()
        {
            return Result?.ToString();
        }

        /// <summary>
        /// Get the transaction count as a BigInteger
        /// </summary>
        public BigInteger? GetCount()
        {
            var hex = GetCountHex();
            if (string.IsNullOrEmpty(hex)) return null;
            
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            
            if (string.IsNullOrEmpty(hex)) return BigInteger.Zero;
            
            return BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
