using System.Numerics;
using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthStorageValue : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the storage value as a hex string
        /// </summary>
        public string GetValueHex()
        {
            return Result?.ToString();
        }

        /// <summary>
        /// Get the storage value as a BigInteger
        /// </summary>
        public BigInteger? GetValue()
        {
            var hex = GetValueHex();
            if (string.IsNullOrEmpty(hex)) return null;
            
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            
            if (string.IsNullOrEmpty(hex)) return BigInteger.Zero;
            
            return BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
