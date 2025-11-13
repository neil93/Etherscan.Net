using System.Numerics;
using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthGasPriceResult : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the gas price as a hex string (in wei)
        /// </summary>
        public string GetGasPriceHex()
        {
            return Result?.ToString();
        }

        /// <summary>
        /// Get the gas price as a BigInteger (in wei)
        /// </summary>
        public BigInteger? GetGasPrice()
        {
            var hex = GetGasPriceHex();
            if (string.IsNullOrEmpty(hex)) return null;
            
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            
            if (string.IsNullOrEmpty(hex)) return BigInteger.Zero;
            
            return BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Get the gas price in Gwei
        /// </summary>
        public decimal? GetGasPriceGwei()
        {
            var wei = GetGasPrice();
            if (!wei.HasValue) return null;
            
            return (decimal)wei.Value / 1_000_000_000m;
        }
    }
}
