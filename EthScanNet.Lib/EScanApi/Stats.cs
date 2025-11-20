using System;
using System.Threading.Tasks;
using EthScanNet.Lib.Models.ApiRequests.Stats;
using EthScanNet.Lib.Models.ApiResponses.Stats;

namespace EthScanNet.Lib.EScanApi
{
    //TODO: Need to redo this using interfaces for clarity and ease
    public sealed class Stats : BaseApi
    {
        public Stats(EScanClient client) : base(client)
        {
        }
 
        /// <summary>
        /// Get Total Supply of the token or coin on the current subscribed chain
        /// </summary>
        /// <returns></returns>
        public async Task<EScanTotalCoinSupply> GetTotalSupply()
        {
            if (this.Client.Network.IsBsc)
            {
                EScanGetTotalBscCoinSupply getTotalBscCoinSupply = new(this.Client);
                return await getTotalBscCoinSupply.SendAsync();
            }
                
            EScanGetTotalEthCoinSupply getTotalEthCoinSupply = new(this.Client);
            return await getTotalEthCoinSupply.SendAsync();
        }
    }
}