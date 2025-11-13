using Newtonsoft.Json;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthBlock : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the block information as a strongly-typed BlockInfo object
        /// </summary>
        public BlockInfo GetBlockInfo()
        {
            if (Result == null) return null;
            
            var json = JsonConvert.SerializeObject(Result);
            return JsonConvert.DeserializeObject<BlockInfo>(json);
        }
    }
}
