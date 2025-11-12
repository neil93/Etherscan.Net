using EthScanNet.Lib.Models.ApiRequests.Proxy;
using EthScanNet.Lib.Models.ApiResponses.Proxy;
using System.Threading.Tasks;

namespace EthScanNet.Lib.EScanApi
{
    public sealed class Proxy : BaseApi
    {
        public Proxy(EScanClient client) : base(client)
        {
        }
        
        public async Task<EScanEthCurrentBlock> CurrentBlock()
        {
            EScanEthBlockNumber ethBlockNumber = new(this.Client);
            return await ethBlockNumber.SendAsync();
        }
    }
}