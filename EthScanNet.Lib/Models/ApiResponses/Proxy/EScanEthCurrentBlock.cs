using System.Numerics;
using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthCurrentBlock : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the block number as a hex string
        /// </summary>
        public string GetBlockNumberHex()
        {
            return Result?.ToString();
        }

        /// <summary>
        /// Get the block number as a BigInteger
        /// </summary>
        public BigInteger? GetBlockNumber()
        {
            var hex = GetBlockNumberHex();
            if (string.IsNullOrEmpty(hex)) return null;
            
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            
            return BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
    }
}